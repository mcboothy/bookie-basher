using BookieBaher.DataMiner.IO;
using BookieBasher.Core;
using BookieBasher.Core.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BookieBaher.DataMiner
{
    public static class MatchUpdater
    {
        public static async Task InsertFixtures(DbContextOptions<BBDBContext> options, JSProcessMatches result, FrameworkService service, string queue)
        {
            using (BBDBContext context = new BBDBContext(options))
            {
                // obtain the season id as the result's eason id is not populated at this stage
                var dbSeason = await context.Season.Include(s => s.Competition) // explicity load the Competition
                                                   .FirstOrDefaultAsync(s => s.Year == result.Season.Year &&
                                                                             s.CompetitionId == result.Season.CompetitionId);
                // get unique team names from the result
                var teams = result.Matches.SelectMany(m => new List<string> { m.HomeTeam, m.AwayTeam })
                                          .Distinct()
                                          .ToList();

                // query the database for the teams in this league
                var dbTeams = await context.Team.Where(t => teams.Contains(t.Name)).ToListAsync();

                // save the fixtures
                foreach (JSMatch fixture in result.Matches)
                {
                    context.Match.Add(ParseMatch(fixture, dbTeams, dbSeason));
                }

                await context.SaveChangesAsync();

                service.SendMessage(Message.Create(dbSeason.ToJSSeason(), "update-season"), queue);
            }
        }

        public static async Task UpdateMatch(DbContextOptions<BBDBContext> options, JSProcessMatch result, FrameworkService service, string queue)
        {
            using (BBDBContext context = new BBDBContext(options))
            {
                Match match = null;
                 
                if (result.MatchStats == null)
                {
                    match = await context.Match.Include((m) => m.Season)
                                               .Include((m) => m.Season.Competition)
                                               .FirstOrDefaultAsync(m => m.MatchId == result.Request.Id);

                    match.Status = "Fixture";
                }
                else
                {
                    match = await context.Match.Include((m) => m.HomeTeamStats)
                                               .Include((m) => m.AwayTeamStats)
                                               .Include((m) => m.Season)
                                               .Include((m) => m.Season.Competition)
                                               .FirstOrDefaultAsync(m => m.MatchId == result.Request.Id);

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

                if (!(await context.Match.AnyAsync((m) => m.SeasonId == match.SeasonId &&
                                                          m.Status == "Updating")))
                {
                    var dbSeason = await context.Season.FirstAsync(s => s.SeasonId == match.SeasonId);

                    if (await context.Match.AnyAsync((m) => m.SeasonId == match.SeasonId &&
                                                          m.LastUpdated > dbSeason.LastUpdated))
                    {
                        service.SendMessage(Message.Create(dbSeason.ToJSSeason(), "update-averages"), queue);
                    }

                    dbSeason.Status = "Updated";
                    dbSeason.LastUpdated = DateTime.Now;
                    context.Season.Update(dbSeason);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static Match ParseMatch(JSMatch match, List<Team> teams, Season season)
        {
            return new Match()
            {
                DateTime = DateTime.Parse(match.DateTime),
                HomeTeam = teams.First(t => t.Name == match.HomeTeam),
                AwayTeam = teams.First(t => t.Name == match.AwayTeam),
                SeasonId = season.SeasonId,
                Status = "Fixture",
                FsmatchId = match.FSMatchID,
                Postponed = match.Postponed ? (sbyte)1 : (sbyte)0,
                LastUpdated = DateTime.Now,
                AwayTeamStats = new Matchstats(),
                HomeTeamStats = new Matchstats()
            };
        }
    }
}
