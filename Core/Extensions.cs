using BookieBasher.Core.Database;
using BookieBasher.Core.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookieBasher.Core
{
    public static class Extensions
    {
        public static string ToJSON(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static byte[] Encode(this object obj)
        {
            string json = obj is string 
                        ? (string) obj 
                        : JsonConvert.SerializeObject(obj);

            return Encoding.UTF8.GetBytes(json);
        }

        public static T Decode<T>(this byte[] bytes)
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static object Decode(this byte[] bytes, Type type)
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(json, type);
        }

        public static T Decode<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static TeamAlias FindAlias(this Team team, string name)
        {
            return team.TeamAlias.AsEnumerable().FirstOrDefault(a => CompareTeamNames(a.Alias, name));
        }

        public static bool IsKnownBy(this Team team, string name)
        {
            return team.TeamAlias.AsEnumerable().Any(a => CompareTeamNames(a.Alias, name));
        }

        public static bool ContainsAlias(this Team team, List<string> names)
        {
            foreach (var alias in team.TeamAlias)
            {
                foreach (string name in names)
                {
                    if (CompareTeamNames(name, alias.Alias))
                        return true;
                }
            }

            return false;
        }

        public static JSSeason ToJSSeason(this Season season)
        {
            return new JSSeason()
            {
                SeasonId = season.SeasonId,
                CompetitionId = season.CompetitionId,
                Status = season.Status,
                Year = season.Year,
                Competition = season.Competition?.ToJSCompetition()
            };
        }

        public static JSCompetition ToJSCompetition(this Competition competition)
        {
            return new JSCompetition()
            {
                CompetitionId = competition.CompetitionId,
                DefaultAlias = competition.CompetitionAlias.FirstOrDefault(a => a.IsDefault == 1)?.Name,
                FlashScoreUrl = competition.FlashScoreUrl,
                SoccerWikiId = competition.SoccerWikiId,
                Sponsor = competition.Sponsor,
                YearFounded = competition.YearFounded
            };
        }

        private static bool CompareTeamNames(string team1, string team2)
        {
            if (string.IsNullOrWhiteSpace(team1) ||
                string.IsNullOrWhiteSpace(team2))
                return false;

            string upper1 = team1.ToUpper();
            string upper2 = team2.ToUpper();

            if (upper1 == upper2)
                return true;

            if (upper1.Contains(upper2) ||
                upper2.Contains(upper1))
            {
                return true;
            }

            return false;
        }
    }
}
