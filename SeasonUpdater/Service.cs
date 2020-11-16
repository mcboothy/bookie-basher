using BookieBasher.Core;
using BookieBasher.Core.Database;
using BookieBasher.Core.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client.Events;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BookieBaher.SeasonUpdater
{
    public class Service : FrameworkService
    {
        public static string Name = "Season Updater";

        protected override bool HasOutbound => true;

        protected override async Task<bool> OnMessageRecieved(object sender, BasicDeliverEventArgs args)
        {
            if (args.BasicProperties.ContentType == null)
                throw new ArgumentNullException(nameof(args.BasicProperties.ContentType));

            switch (args.BasicProperties.ContentType)
            {
                case "update":
                    await UpdateSeasons();
                    return true;

                case "update-season":
                    await UpdateSeason(args.Body.Decode<JSSeason>());
                    return true;

                default:
                    throw new ArgumentException($"Invalid content type {args.BasicProperties.ContentType}");
            }
        }

        protected override void ReadConfig(IConfiguration config)
        {
            inboundQueue = config.GetValue<string>("UpdateQueue");
            outboundQueue = config.GetValue<string>("ScrapeQueue");
        }

        private async Task UpdateSeasons()
        {
            Log("Updating....");

            using (BBDBContext context = new BBDBContext(options))
            {
                string season = GetYear();

                foreach (Competition competition in await context.Competition.ToListAsync())
                {
                    Season dbSeason = await context.Season.FirstOrDefaultAsync(s => s.Year == season &&
                                                                                    s.CompetitionId == competition.CompetitionId);

                    if (dbSeason == null)
                    {
                        await CreateSeason(context, season, competition.CompetitionId);
                    }
                    else 
                    {
                        switch (dbSeason.Status)
                        {
                            case "Updated":
                                await UpdateSeason(context, dbSeason);
                                break;

                            default:
                                if (dbSeason.LastUpdated < DateTime.Now.AddHours(-0.5))
                                {
                                    bool hasFixtures = await context.Match.Where(m => m.SeasonId == dbSeason.SeasonId &&
                                                                                      m.Status == "Fixture")
                                                                           .AnyAsync();

                                    if (!hasFixtures)
                                    {
                                        dbSeason.LastUpdated = DateTime.Now;
                                        dbSeason.Status = "Creating";
                                        context.Season.Add(dbSeason);
                                        await context.SaveChangesAsync();

                                        SendMessage(Message.Create(dbSeason.ToJSSeason(), "request-fixtures"));
                                    }
                                }
                                break;
                        }                                                    
                    }
                }
            }
        }

        private async Task CreateSeason(BBDBContext context, string season, int competitionID)
        {
            Season dbSeason = new Season()
            {
                CompetitionId = competitionID,
                Year = season,
                Status = "Creating",
                LastUpdated = DateTime.Now
            };

            context.Season.Add(dbSeason);
            await context.SaveChangesAsync();

            SendMessage(Message.Create(dbSeason.ToJSSeason(), "request-fixtures"));
        }

        private async Task UpdateSeason(JSSeason season)
        {
            Log($"Updating season {season.Competition.DefaultAlias}");

            using (BBDBContext context = new BBDBContext(options))
            {
                Season dbSeason = await context.Season.FirstAsync(s => s.SeasonId == season.SeasonId);
                await UpdateSeason(context, dbSeason);
            }
        }

        private async Task UpdateSeason(BBDBContext context, Season dbSeason)
        {
            // Get all fixtures that haven't been scraped ordered asscending
            var matchesToUpdate = context.Match.Where(m => m.SeasonId == dbSeason.SeasonId &&
                                                         m.Status != "Result" &&
                                                         m.DateTime <= DateTime.Now.AddHours(-2))
                                                .OrderBy(m => m.DateTime);

            if (matchesToUpdate.Any())
            {
                dbSeason.Status = "Updating";
                dbSeason.LastUpdated = DateTime.Now;
                context.Season.Update(dbSeason);

                foreach (Match match in matchesToUpdate)
                {
                    match.Status = "Updating";
                    context.Match.Update(match);

                    var request = new JSRequestMatch()
                    {
                        MatchId = match.FsmatchId,
                        Id = match.MatchId
                    };

                    SendMessage(Message.Create(request, "request-match"));
                }
            }
            else
            {
                dbSeason.Status = "Updated";
                dbSeason.LastUpdated = DateTime.Now;
                context.Season.Update(dbSeason);
            }

            await context.SaveChangesAsync();
        }

        private static string GetYear()
        {
            int year = DateTime.Now.Year;

            string season;

            if (DateTime.Now.Month > 6)
                season = year + "/" + (year + 1);
            else
                season = (year - 1) + "/" + year;

            return season;
        }
    }
}
