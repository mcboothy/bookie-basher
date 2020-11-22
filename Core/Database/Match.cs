using System;
using System.Collections.Generic;

#nullable disable

namespace BookieBasher.Core.Database
{
    public partial class Match
    {
        public int MatchId { get; set; }
        public int SeasonId { get; set; }
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public int? HomeTeamStatsId { get; set; }
        public int? AwayTeamStatsId { get; set; }
        public string Status { get; set; }
        public DateTime DateTime { get; set; }
        public string FsmatchId { get; set; }
        public sbyte Postponed { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual Team AwayTeam { get; set; }
        public virtual MatchStat AwayTeamStats { get; set; }
        public virtual Team HomeTeam { get; set; }
        public virtual MatchStat HomeTeamStats { get; set; }
        public virtual Season Season { get; set; }
    }
}
