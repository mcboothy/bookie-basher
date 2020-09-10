using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Fixtures
    {
        public int MatchId { get; set; }
        public int SeasonId { get; set; }
        public string HomeTeam { get; set; }
        public int HomeTeamId { get; set; }
        public string AwayTeam { get; set; }
        public int AwayTeamId { get; set; }
        public int? HomeTeamStatsId { get; set; }
        public int? AwayTeamStatsId { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
    }
}
