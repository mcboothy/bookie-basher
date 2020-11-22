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

        public static T Decode<T>(this ReadOnlyMemory<byte> bytes)
        {
            string json = Encoding.UTF8.GetString(bytes.ToArray());
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static object Decode(this ReadOnlyMemory<byte> bytes, Type type)
        {
            string json = Encoding.UTF8.GetString(bytes.ToArray());
            return JsonConvert.DeserializeObject(json, type);
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
            return team.TeamAliases.AsEnumerable().FirstOrDefault(a => CompareTeamNames(a.Alias, name));
        }

        public static bool IsKnownBy(this Team team, string name)
        {
            return team.TeamAliases.AsEnumerable().Any(a => CompareTeamNames(a.Alias, name));
        }

        public static bool ContainsAlias(this Team team, List<string> names)
        {
            foreach (var alias in team.TeamAliases)
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
                DefaultAlias = competition.CompetitionAliases.FirstOrDefault(a => a.IsDefault == 1)?.Name,
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

        public static string ToDetailedString(this Exception ex)
        {
            if (ex is null)
                throw new ArgumentNullException(nameof(ex));

            StringBuilder entry = new StringBuilder();
            entry.AppendLine("******************** ERROR ***********************");
            entry.AppendLine("Exception");
            entry.AppendLine(ex.GetType().ToString());
            entry.AppendLine();

            if (ex.Message != null)
            {
                entry.AppendLine("Message");
                entry.AppendLine(ex.Message);
                entry.AppendLine();
            }

            if (ex.Source != null)
            {
                entry.AppendLine("Source");
                entry.AppendLine(ex.Source);
                entry.AppendLine();
            }

            if (ex.Data != null)
            {
                entry.AppendLine("StackTrace");
                entry.AppendLine(ex.StackTrace);
                entry.AppendLine();
            }

            LogData(ex, ref entry, 0);

            if (ex.InnerException != null)
            {
                entry.AppendLine("======================= Inner Exceptions ======================");
                LogInnerExceptions(ex.InnerException, ref entry, 1);
                entry.AppendLine("===============================================================");
            }

            entry.AppendLine("******************** END OF ERROR ***********************");

            return entry.ToString();
        }

        private static void LogInnerExceptions(Exception e, ref StringBuilder entry, int depth)
        {
            if (e != null)
            {
                string tabs = GetTabs(depth);

                entry.AppendLine(tabs + "----------- Exception (" + depth + ") ----------------");

                entry.AppendLine(tabs + "Exception");
                entry.AppendLine(tabs + e.GetType().ToString());
                entry.AppendLine();

                if (e.Message != null)
                {
                    entry.AppendLine(tabs + "Message");
                    entry.AppendLine(tabs + e.Message);
                    entry.AppendLine();
                }

                if (e.Source != null)
                {
                    entry.AppendLine(tabs + "Source");
                    entry.AppendLine(tabs + e.Source);
                    entry.AppendLine();
                }

                if (e.StackTrace != null)
                {
                    entry.AppendLine(tabs + "StackTrace");
                    entry.AppendLine(tabs + e.StackTrace.Replace("\n", "\n" + tabs));
                    entry.AppendLine();
                }

                LogData(e, ref entry, depth);

                entry.AppendLine(tabs + "----------- Exception (" + depth + ")-----------------");

                if (e.InnerException != null)
                {
                    LogInnerExceptions(e.InnerException, ref entry, ++depth);
                }
            }
        }

        private static void LogData(Exception e, ref StringBuilder entry, int depth)
        {
            if (HasData(e))
            {
                string tabs = GetTabs(depth);

                entry.AppendLine(tabs + "Data");

                foreach (var data in e.Data.Values)
                {
                    entry.AppendLine(tabs + data.ToString());
                }
            }
        }

        private static bool HasData(Exception e)
        {
            return e.Data != null && e.Data.Count > 0;
        }

        private static string GetTabs(int depth)
        {
            string tabs = "";

            for (int n = 0; n < depth; n++)
            {
                tabs += "\t";
            }

            return tabs;
        }
    }
}
