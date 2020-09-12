﻿using BookieBasher.Core;
using BookieBasher.Core.Database;
using BookieBasher.Core.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BookieBaher.SeasonUpdater
{
    public class Service : FrameworkService
    {
        public static string Name = "Match Processor.";

        protected override bool HasOutbound => true;

        protected override async Task<bool> OnMessageRecieved(object sender, BasicDeliverEventArgs args)
        {
            if (args.BasicProperties.ContentType == null)
                throw new ArgumentNullException(nameof(args.BasicProperties.ContentType));

            switch (args.BasicProperties.ContentType)
            {
                case "process-fixtures":
                    await InsertFixtures(args.Body.Decode<JSProcessMatches>());
                    return true;

                case "process-match":
                    await UpdateMatch(args.Body.Decode<JSProcessMatch>());
                    return true;

                default:
                    throw new ArgumentException($"Invalid content type {args.BasicProperties.ContentType}");
            }
        }

        protected override void ReadConfig(IConfiguration config)
        {
            inboundQueue = config.GetValue<string>("MatchProcessQueue");
            outboundQueue = config.GetValue<string>("UpdateQueue");
        }

        private async Task InsertFixtures(JSProcessMatches result)
        {
            using (BBDBContext context = new BBDBContext(options))
            {
                // obtain the season id as the result's eason id is not populated at this stage
                var dbSeason = await context.Season.Include(s => s.Competition) // explicity load the Competition
                                                    .Include(s => s.Competition.Competitionalias) // explicity load the Competition Aliases
                                                   .FirstOrDefaultAsync(s => s.Year == result.Season.Year &&
                                                                             s.CompetitionId == result.Season.CompetitionId);
                if (dbSeason == null)
                    throw new Exception($"Cannot find season { result.Season.Competition.Names}");

                // fail the season if no fixtures where found
                if (result.Matches == null ||  !result.Matches.Any())
                {
                    dbSeason.Status = "Failed";
                    context.Season.Update(dbSeason);
                    await context.SaveChangesAsync();
                    return;
                }

                // get unique team names from the result
                var teams = result.Matches.SelectMany(m => new List<string> { m.HomeTeam, m.AwayTeam })
                                          .Distinct()
                                          .ToList();

                // query the database for the teams in this league
                var dbTeams = (await context.Team.Include(t => t.TeamAlias).ToListAsync())
                                                 .Where(t => t.ContainsAlias(teams));

                HashSet<JSMatch> failedMatches = new HashSet<JSMatch>();

                // save the fixtures
                foreach (JSMatch fixture in result.Matches)
                {
                    var homeTeam = dbTeams.FirstOrDefault(t => t.IsKnownBy(fixture.HomeTeam));
                    var awayTeam = dbTeams.FirstOrDefault(t => t.IsKnownBy(fixture.AwayTeam));

                    // can't resolve team(s) so throw error
                    if (homeTeam == null || awayTeam == null)
                    {
                        UnknownTeams unknownTeams = new UnknownTeams()
                        {
                            Request = result.ToJSON(),
                            SeasonId = result.Season.SeasonId
                        };

                        context.UnknownTeams.Add(unknownTeams);
                        await context.SaveChangesAsync();

                        throw new Exception($"Unknow teams in season {result.Season.Competition.DefaultAlias}");
                    }

                    var match = ParseMatch(fixture, homeTeam, awayTeam, dbSeason);
                    context.Match.Add(match);
                }

                await context.SaveChangesAsync();

                SendMessage(Message.Create(dbSeason.ToJSSeason(), "update-season"));
            }
        }

        private async Task UpdateMatch(JSProcessMatch result)
        {
            using (BBDBContext context = new BBDBContext(options))
            {
                Match match = null;

                if (result.MatchStats == null)
                {
                    match = await context.Match.Include((m) => m.Season)
                                               .Include((m) => m.Season.Competition)
                                               .FirstAsync(m => m.MatchId == result.Request.Id);

                    match.Status = "Fixture";
                }
                else
                {
                    match = await context.Match.Include((m) => m.HomeTeamStats)
                                               .Include((m) => m.AwayTeamStats)
                                               .Include((m) => m.Season)
                                               .Include((m) => m.Season.Competition)
                                               .FirstAsync(m => m.MatchId == result.Request.Id);

                    match.HomeTeamStats.FirstHalfGoals = result.MatchStats.Home.FirstHalf.Goals;
                    match.HomeTeamStats.FirstHalfCards = result.MatchStats.Home.FirstHalf.Cards;
                    match.HomeTeamStats.TotalGoals = result.MatchStats.Home.FirstHalf.Goals +
                                                     result.MatchStats.Home.SecondHalf.Goals;
                    match.HomeTeamStats.TotalCards = result.MatchStats.Home.FirstHalf.Cards +
                                                     result.MatchStats.Home.SecondHalf.Cards;

                    match.AwayTeamStats.FirstHalfGoals = result.MatchStats.Away.FirstHalf.Goals;
                    match.AwayTeamStats.FirstHalfCards = result.MatchStats.Away.FirstHalf.Cards;
                    match.AwayTeamStats.TotalGoals = result.MatchStats.Away.FirstHalf.Goals +
                                                     result.MatchStats.Away.SecondHalf.Goals;
                    match.AwayTeamStats.TotalCards = result.MatchStats.Away.FirstHalf.Cards +
                                                     result.MatchStats.Away.SecondHalf.Cards;
                    match.Status = "Result";
                    match.LastUpdated = DateTime.Now;

                    context.Matchstats.Update(match.HomeTeamStats);
                    context.Matchstats.Update(match.AwayTeamStats);
                }

                context.Match.Update(match);
                await context.SaveChangesAsync();
            }
        }

        private Match ParseMatch(JSMatch match, Team homeTeam, Team awayTeam, Season season)
        {
            return new Match()
            {
                DateTime = match.DateTime,
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                SeasonId = season.SeasonId,
                Status = "Fixture",
                FsmatchId = match.FSMatchID,
                Postponed = match.Postponed ? (sbyte)1 : (sbyte)0,
                LastUpdated = DateTime.Now,
                AwayTeamStats = new MatchStats(),
                HomeTeamStats = new MatchStats()
            };
        }
    }
}
