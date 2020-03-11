using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BookieBaher.DataMiner.IO;
using BookieBasher.Core;
using BookieBasher.Core.Database;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookieBaher.DataMiner
{
    public static class TeamUpdater
    {
        public static async Task UpdateTeams(DbContextOptions<BBDBContext> options, JSProcessTeams result, FrameworkService service, string queue)
        {
            using (BBDBContext context = new BBDBContext(options))
            {
                List<string> teamNames = result.Teams.Select(t => t.Name).ToList();
                List<Team> storedTeams = await context.Team.Where(t => teamNames.Contains(t.Name))
                                                           .ToListAsync();

                foreach(JSTeam team in result.Teams)
                {
                    Team storedTeam = storedTeams.FirstOrDefault(t => t.Name == team.Name);

                    if (storedTeam == null)
                    {
                        context.Team.Add(new Team()
                        {
                            Name = team.Name,
                            LogoUrl = team.Url
                        });
                    }
                }

                await context.SaveChangesAsync();

                var dbSeason = context.Season.Include(s => s.Competition)
                                             .First(s => s.CompetitionId == result.Season.CompetitionId &&
                                                         s.Year == result.Season.Year);

                service.SendMessage(Message.Create(dbSeason.ToJSSeason(), "request-fixtures"), queue);              
            }
        }
    }
}
