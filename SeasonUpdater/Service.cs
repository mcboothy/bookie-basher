﻿using BookieBasher.Core;
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

                case "update-status":
                    await UpdateStatus(args.Body.Decode<JSSeason>());
                    return true;

                default:
                    throw new ArgumentException($"Invalid content type {args.BasicProperties.ContentType}");
            }
        }

        private async Task UpdateStatus(JSSeason season)
        {
            using (BBDBContext context = new BBDBContext(options))
            {
                Season dbSeason = await context.Seasons.FirstAsync(s => s.SeasonId == season.SeasonId);

                if (dbSeason.Status == "Updating")
                {
                    dbSeason.Status = "Updated";
                    await context.SaveChangesAsync();
                }
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

                foreach (Competition competition in await context.Competitions.Include(c => c.CompetitionAliases)
                                                                             .ToListAsync())
                {
                    Season dbSeason = await context.Seasons.Include(s => s.Competition)
                                                           .ThenInclude(c => c.CompetitionAliases)
                                                           .FirstOrDefaultAsync(s => s.Year == season &&
                                                                                     s.CompetitionId == competition.CompetitionId);

                    if (dbSeason == null)
                    {
                        await CreateSeason(context, season, competition);
                    }
                    else 
                    {
                        switch (dbSeason.Status)
                        {
                            case "Updated":
                                await UpdateSeason(context, dbSeason);
                                break;

                            case "Failed":
                                bool hasFixtures = await context.Matches.Where(m => m.SeasonId == dbSeason.SeasonId &&
                                                                                      m.Status == "Fixture")
                                                                           .AnyAsync();
                                if (!hasFixtures)
                                {
                                    dbSeason.LastUpdated = DateTime.Now;
                                    dbSeason.Status = "Creating";
                                    await context.SaveChangesAsync();

                                    SendMessage(Message.Create(dbSeason.ToJSSeason(), "request-fixtures"));
                                }
                                break;
                        }                                                    
                    }
                }
            }
        }

        private async Task CreateSeason(BBDBContext context, string season, Competition competition)
        {
            Season dbSeason = new Season()
            {
                Competition = competition,
                Year = season,
                Status = "Creating",
                LastUpdated = DateTime.Now
            };

            context.Seasons.Add(dbSeason);
            await context.SaveChangesAsync();

            SendMessage(Message.Create(dbSeason.ToJSSeason(), "request-fixtures"));
        }

        private async Task UpdateSeason(JSSeason season)
        {
            Log($"Updating season {season.Competition.DefaultAlias}");

            using (BBDBContext context = new BBDBContext(options))
            {
                Season dbSeason = await context.Seasons.FirstAsync(s => s.SeasonId == season.SeasonId);
                await UpdateSeason(context, dbSeason);
            }
        }

        private async Task UpdateSeason(BBDBContext context, Season dbSeason)
        {
            // Get all fixtures that haven't been scraped ordered asscending
            var matchesToUpdate = context.Matches.Where(m => m.SeasonId == dbSeason.SeasonId &&
                                                         m.Status != "Result" &&
                                                         m.DateTime <= DateTime.Now.AddHours(-2))
                                                .OrderBy(m => m.DateTime);

            if (matchesToUpdate.Any())
            {
                dbSeason.Status = "Updating";
                dbSeason.LastUpdated = DateTime.Now;
                context.Seasons.Update(dbSeason);

                foreach (Match match in matchesToUpdate)
                {
                    match.Status = "Updating";
                    context.Matches.Update(match);

                    var teamAliases = await context.TeamAliases.Where(ta => ta.TeamId == match.AwayTeamId ||
                                                                          ta.TeamId == match.HomeTeamId).ToListAsync();

                    var homeTeam = teamAliases.FirstOrDefault(ta => ta.TeamId == match.HomeTeamId && ta.IsDefault == 1);

                    if (homeTeam == null)
                        homeTeam = teamAliases.FirstOrDefault(ta => ta.TeamId == match.HomeTeamId);

                    var awayTeam = teamAliases.FirstOrDefault(ta => ta.TeamId == match.AwayTeamId && ta.IsDefault == 1);

                    if (awayTeam == null)
                        awayTeam = teamAliases.FirstOrDefault(ta => ta.TeamId == match.AwayTeamId);

                    var request = new JSRequestMatch()
                    {
                        MatchId = match.FsmatchId,
                        Id = match.MatchId, 
                        HomeTeam = homeTeam?.Alias,
                        AwayTeam = awayTeam?.Alias
                    };

                    SendMessage(Message.Create(request, "request-match"));
                }

                SendMessage(Message.Create(dbSeason.ToJSSeason(), "update-status"));
            }
            else
            {
                dbSeason.Status = "Updated";
                dbSeason.LastUpdated = DateTime.Now;
                context.Seasons.Update(dbSeason);
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
