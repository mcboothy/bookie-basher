using BookieBaher.DataMiner.IO;
using BookieBasher.Core.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBaher.DataMiner
{
    public static class Extensions
    {
        public static JSSeason ToJSSeason(this Season season)
        {
            JSCompetition competition = null;

            if (season.Competition != null)
            {
                competition = new JSCompetition()
                {
                    CompetitionId = season.Competition.CompetitionId,
                    Name = season.Competition.Name,
                    Url = season.Competition.Url
                };
            }

            return new JSSeason()
            {
                SeasonId = season.SeasonId,
                CompetitionId = season.CompetitionId,
                Status = season.Status,
                Year = season.Year,
                Competition = competition
            };
        }
    }
}
