using BookieBasher.Core;
using BookieBasher.Core.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BookieBaher.DataMiner.IO;

namespace BookieBaher.DataMiner
{
    public class SeasonUpdater
    {
        public static async Task UpdateSeasons(DbContextOptions<BBDBContext> options, FrameworkService service, string queue)
        {
            using (BBDBContext context = new BBDBContext(options))
            {
                string season = GetYear();

                foreach (Competition competition in await context.Competition.ToListAsync())
                {
                    Season dbSeason = await context.Season.FirstOrDefaultAsync(s => s.Year == season &&
                                                                                    s.CompetitionId == competition.CompetitionId);

                    if (dbSeason == null)
                    {
                        await CreateSeason(context, season, competition.CompetitionId, service, queue);
                    }
                    else
                    {
                        await UpdateSeason(context, dbSeason, service, queue);
                    }
                }
            }
        }

        public static async Task UpdateSeason(DbContextOptions<BBDBContext> options, JSSeason season, FrameworkService service, string queue)
        {
            using (BBDBContext context = new BBDBContext(options))
            {
                Season dbSeason = await context.Season.FirstAsync(s => s.SeasonId == season.SeasonId);
                await UpdateSeason(context, dbSeason, service, queue);
            }
        }

        public static async Task UpdateAllAverages(DbContextOptions<BBDBContext> options, FrameworkService service, string queue)
        {
            using (BBDBContext context = new BBDBContext(options))
            {
                string season = GetYear();

                foreach (Competition competition in await context.Competition.ToListAsync())
                {
                    Season dbSeason = await context.Season.FirstOrDefaultAsync(s => s.Year == season &&
                                                                                    s.CompetitionId == competition.CompetitionId);

                    if (dbSeason != null)
                    {
                        service.SendMessage(Message.Create(dbSeason.ToJSSeason(), "update-averages"), queue);
                    }
                }
            }
        }
         
        public static async Task UpdateAverages(DbContextOptions<BBDBContext> options, JSSeason season)
        {
            using (BBDBContext context = new BBDBContext(options))
            {
                var leagueTeams = context.Leagueteams.Where(lt => lt.SeasonId == season.SeasonId);

                foreach (var team in await leagueTeams.ToListAsync())
                {
                    var averages = await context.Averagestat.Where(avg => avg.TeamId == team.TeamId &&
                                                                          avg.SeasonId == season.SeasonId).ToListAsync();

                    int homeGoals_Overall = 0, awayGoals_Overall = 0;
                    int homeGoals_First = 0, awayGoals_First = 0;
                    int homeGoals_Second = 0, awayGoals_Second = 0;
                    int homeCards_Overall = 0, awayCards_Overall = 0;
                    int homeCards_First = 0, awayCards_First = 0;
                    int homeCards_Second = 0, awayCards_Second = 0;
                    int overallCards = 0, overallGoals = 0;
                    int firstCards = 0, firstGoals = 0;
                    int secondCards = 0, secondGoals = 0;
                    int gameCount = 0, homeGameCount = 0, awayGameCount = 0;
                    int points = 0, homePoints = 0, awayPoints = 0;

                    foreach (Match match in await context.Match.Where(m => m.Status == "Result" &&
                                                                         (m.HomeTeamId == team.TeamId ||
                                                                          m.AwayTeamId == team.TeamId))
                                                               .Include(m => m.HomeTeamStats)
                                                               .Include(m => m.AwayTeamStats)
                                                               .OrderByDescending(m => m.DateTime)
                                                               .ToListAsync())
                    {
                        bool isHome = IsHome(match, team.TeamId);
                        bool homeUpdated = false;
                        bool awayUpdated = false;

                        if (isHome)
                        {
                            homeGoals_Overall += GetValue(match, isGoals: true, firstHalf: false, homeAway: true);
                            homeGoals_First += GetValue(match, isGoals: true, firstHalf: true, homeAway: true);
                            homeGoals_Second += GetValue(match, isGoals: true, firstHalf: false, homeAway: true) -
                                                 GetValue(match, isGoals: true, firstHalf: true, homeAway: true);

                            homeCards_Overall += GetValue(match, isGoals: false, firstHalf: false, homeAway: true);
                            homeCards_First += GetValue(match, isGoals: false, firstHalf: true, homeAway: true);
                            homeCards_Second += GetValue(match, isGoals: false, firstHalf: false, homeAway: true) -
                                                 GetValue(match, isGoals: false, firstHalf: true, homeAway: true);

                            homeGameCount++;

                            int homeGoals = GetValue(match, isGoals: true, firstHalf: false, homeAway: true);
                            int awayGoals = GetValue(match, isGoals: true, firstHalf: false, homeAway: false);

                            if (homeGoals == awayGoals)
                            {
                                homePoints++;
                                points++;
                            }
                            else if (homeGoals > awayGoals)
                            {
                                homePoints += 3;
                                points += 3;
                            }

                            homeUpdated = true;
                        }
                        else
                        {
                            awayGoals_Overall += GetValue(match, isGoals: true, firstHalf: false, homeAway: false);
                            awayGoals_First += GetValue(match, isGoals: true, firstHalf: true, homeAway: false);
                            awayGoals_Second += GetValue(match, isGoals: true, firstHalf: false, homeAway: false) -
                                                 GetValue(match, isGoals: true, firstHalf: true, homeAway: false);

                            awayCards_Overall += GetValue(match, isGoals: false, firstHalf: false, homeAway: false);
                            awayCards_First += GetValue(match, isGoals: false, firstHalf: true, homeAway: false);
                            awayCards_Second += GetValue(match, isGoals: false, firstHalf: false, homeAway: false) -
                                                 GetValue(match, isGoals: false, firstHalf: true, homeAway: false);

                            awayGameCount++;

                            int homeGoals = GetValue(match, isGoals: true, firstHalf: false, homeAway: true);
                            int awayGoals = GetValue(match, isGoals: true, firstHalf: false, homeAway: false);

                            if (homeGoals == awayGoals)
                            {
                                awayPoints++;
                                points++;
                            }
                            else if (homeGoals < awayGoals)
                            {
                                awayPoints += 3;
                                points += 3;
                            }

                            awayUpdated = true;
                        }

                        gameCount++;

                        overallGoals += GetValue(match, isGoals: true, firstHalf: false, isHome);
                        overallCards += GetValue(match, isGoals: false, firstHalf: false, isHome);

                        firstGoals += GetValue(match, isGoals: true, firstHalf: true, isHome);
                        firstCards += GetValue(match, isGoals: false, firstHalf: true, isHome);

                        secondGoals += GetValue(match, isGoals: true, firstHalf: false, isHome) -
                                        GetValue(match, isGoals: true, firstHalf: true, isHome);
                        secondCards += GetValue(match, isGoals: false, firstHalf: false, isHome) -
                                        GetValue(match, isGoals: false, firstHalf: true, isHome);

                        if (gameCount % 5 == 0)
                        {
                            AddOrUpdate(context, new Averagestat()
                            {
                                SeasonId = season.SeasonId,
                                TeamId = team.TeamId,
                                TotalGoal = Average((double)overallGoals / (double)gameCount),
                                TotalCard = Average((double)overallCards / (double)gameCount),
                                FirstHalfGoal = Average((double)firstGoals / (double)gameCount),
                                FirstHalfCard = Average((double)firstCards / (double)gameCount),
                                SecondHalfGoal = Average((double)secondGoals / (double)gameCount),
                                SecondHalfCard = Average((double)secondCards / (double)gameCount),
                                PointPercentage = Average((double)points / (double)(gameCount * 3)) * 100,
                                Type = "Overall-" + gameCount,
                            }, averages);
                        }

                        if (homeUpdated && homeGameCount % 5 == 0)
                        {
                            AddOrUpdate(context, new Averagestat()
                            {
                                SeasonId = season.SeasonId,
                                TeamId = team.TeamId,
                                TotalGoal = Average((double)homeGoals_Overall / (double)homeGameCount),
                                TotalCard = Average((double)homeCards_Overall / (double)homeGameCount),
                                FirstHalfGoal = Average((double)homeGoals_First / (double)homeGameCount),
                                FirstHalfCard = Average((double)homeCards_First / (double)homeGameCount),
                                SecondHalfGoal = Average((double)homeGoals_Second / (double)homeGameCount),
                                SecondHalfCard = Average((double)homeCards_Second / (double)homeGameCount),
                                PointPercentage = Average((double)homePoints / (double)(homeGameCount * 3)) * 100,
                                Type = "Home-" + homeGameCount,
                            }, averages);

                            homeUpdated = false;
                        }

                        if (awayUpdated && awayGameCount % 5 == 0)
                        {
                            AddOrUpdate(context, new Averagestat()
                            {
                                SeasonId = season.SeasonId,
                                TeamId = team.TeamId,
                                TotalGoal = Average((double)awayGoals_Overall / (double)awayGameCount),
                                TotalCard = Average((double)awayCards_Overall / (double)awayGameCount),
                                FirstHalfGoal = Average((double)awayGoals_First / (double)awayGameCount),
                                FirstHalfCard = Average((double)awayCards_First / (double)awayGameCount),
                                SecondHalfGoal = Average((double)awayGoals_Second / (double)awayGameCount),
                                SecondHalfCard = Average((double)awayCards_Second / (double)awayGameCount),
                                PointPercentage = Average((double)awayPoints / (double)(awayGameCount * 3)) * 100,
                                Type = "Away-" + awayGameCount,
                            }, averages);

                            awayUpdated = false;
                        }
                    }

                    AddOrUpdate(context, new Averagestat()
                    {
                        SeasonId = season.SeasonId,
                        TeamId = team.TeamId,
                        TotalGoal = Average((double)overallGoals / (double)gameCount),
                        TotalCard = Average((double)overallCards / (double)gameCount),
                        FirstHalfGoal = Average((double)firstGoals / (double)gameCount),
                        FirstHalfCard = Average((double)firstCards / (double)gameCount),
                        SecondHalfGoal = Average((double)secondGoals / (double)gameCount),
                        SecondHalfCard = Average((double)secondCards / (double)gameCount),
                        PointPercentage = Average((double)points / (double)(gameCount * 3)) * 100,
                        Type = "Overall",
                    }, averages);
                }

                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateSeason(BBDBContext context, string season, int competitionID, FrameworkService service, string queue)
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

            service.SendMessage(Message.Create(dbSeason.ToJSSeason(), "request-teams"), queue);
        }

        private static async Task UpdateSeason(BBDBContext context, Season dbSeason, FrameworkService service, string queue)
        {
            var matchesToUpdate = context.Match.Where(m => m.SeasonId == dbSeason.SeasonId &&
                                                         m.Status != "Result" &&
                                                         m.DateTime <= DateTime.Now.AddHours(-2));

            if (matchesToUpdate.Any())
            {
                dbSeason.Status = "Updating";
                dbSeason.LastUpdated = DateTime.Now;
                context.Season.Update(dbSeason);

                foreach(Match match in matchesToUpdate)
                {
                    match.Status = "Updating";
                    context.Match.Update(match);

                    var request = new JSRequestMatch()
                    {
                        MatchId = match.FsmatchId,
                        Id = match.MatchId
                    };

                    service.SendMessage(Message.Create(request, "request-match"), queue);
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

        private static double Average(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return 0.0;

            return value;
        }

        private static bool IsHome(Match m, int id) 
        {
            return id == m.HomeTeamId;
        }

        private static int GetValue(Match m, bool isGoals, bool firstHalf, bool homeAway)
        {
            Matchstats stats = homeAway ? m.HomeTeamStats : m.AwayTeamStats;

            if (isGoals)
            {
                if (firstHalf)
                    return stats.FirstHalfGoals;
                else
                    return stats.TotalGoals;
            }
            else
            {
                if (firstHalf)
                    return stats.FirstHalfCards;
                else
                    return stats.TotalCards;
            }
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

        private static void AddOrUpdate(BBDBContext context, Averagestat stat, List<Averagestat> stats)
        {
            Averagestat dbStat = stats.FirstOrDefault((avg => avg.Type == stat.Type &&
                                                              avg.SeasonId == stat.SeasonId &&
                                                              avg.TeamId == stat.TeamId));

            if (dbStat == null)
            {
                context.Averagestat.Add(stat);
            }
            else
            {
                dbStat.TotalGoal = stat.TotalGoal;
                dbStat.TotalCard = stat.TotalCard;
                dbStat.FirstHalfGoal = stat.FirstHalfGoal;
                dbStat.FirstHalfCard = stat.FirstHalfCard;
                dbStat.SecondHalfGoal = stat.SecondHalfGoal;
                dbStat.SecondHalfCard = stat.SecondHalfCard;
                dbStat.PointPercentage = stat.PointPercentage;
                context.Averagestat.Update(dbStat);
            }
        }

        //public static async Task UpdateStandings(DbContextOptions<BBDBContext> options, JSProcessStandings result)
        //{
        //    using (BBDBContext context = new BBDBContext(options))
        //    {
        //        var dbSeason = await context.Season.FirstAsync(s => s.SeasonId == result.Season.SeasonId);

        //        var teams = context.Leagueteams.Where(lt => lt.SeasonId == result.Season.SeasonId);

        //        foreach (JSLeague league in result.Leagues)
        //        {
        //            var leaguePositions = await context.Leagueposition.Where(lp => lp.SeasonId == dbSeason.SeasonId &&
        //                                                                           lp.Type == league.Type).ToListAsync();

        //            int totalPoints = league.LeaguePositions.Sum(lp => lp.Points);

        //            if (!leaguePositions.Any())
        //            {
        //                foreach (JSLeaguePosition leaguePosition in league.LeaguePositions)
        //                {
        //                    string[] goalsForAgainst = leaguePosition.GoalsForAgains.Split(':');

        //                    context.Leagueposition.Add(new Leagueposition()
        //                    {
        //                        Season = dbSeason,
        //                        Type = league.Type,
        //                        Position = leaguePosition.Position,
        //                        TeamId = teams.First(t => t.Name == leaguePosition.TeamName).TeamId,
        //                        MatchesPlayed = leaguePosition.MatchesPlayed,
        //                        MatchesWon = leaguePosition.MatchesWon,
        //                        MatchesDrawn = leaguePosition.MatchesDrawn,
        //                        MatchesLost = leaguePosition.MatchesLost,
        //                        GolsScored = short.Parse(goalsForAgainst[0]),
        //                        GoalsConceeded = short.Parse(goalsForAgainst[1]),
        //                        Points = leaguePosition.Points,
        //                        PointAverage = Average((double)totalPoints / (double)leaguePosition.Points)
        //                    }); 
        //                }
        //            }
        //            else
        //            {
        //                foreach (JSLeaguePosition leaguePosition in league.LeaguePositions)
        //                {
        //                    string[] goalsForAgainst = leaguePosition.GoalsForAgains.Split(':');

        //                    Leagueposition dbLeaguePosition = leaguePositions.First(lp => lp.TeamId == teams.First(t => t.Name == leaguePosition.TeamName).TeamId);
        //                    dbLeaguePosition.Position = leaguePosition.Position;
        //                    dbLeaguePosition.TeamId = teams.First(t => t.Name == leaguePosition.TeamName).TeamId;
        //                    dbLeaguePosition.MatchesPlayed = leaguePosition.MatchesPlayed;
        //                    dbLeaguePosition.MatchesWon = leaguePosition.MatchesWon;
        //                    dbLeaguePosition.MatchesDrawn = leaguePosition.MatchesDrawn;
        //                    dbLeaguePosition.MatchesLost = leaguePosition.MatchesLost;
        //                    dbLeaguePosition.GolsScored = short.Parse(goalsForAgainst[0]);
        //                    dbLeaguePosition.GoalsConceeded = short.Parse(goalsForAgainst[1]);
        //                    dbLeaguePosition.Points = leaguePosition.Points;
        //                    dbLeaguePosition.PointAverage = Average((double)totalPoints / (double)leaguePosition.Points);
        //                    context.Leagueposition.Update(dbLeaguePosition);
        //                }
        //            }
        //        }

        //        await context.SaveChangesAsync();
        //    }
        //}
    }
}
