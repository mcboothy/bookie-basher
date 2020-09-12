using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class MatchStats
    {
        public MatchStats()
        {
            MatchAwayTeamStats = new HashSet<Match>();
            MatchHomeTeamStats = new HashSet<Match>();
        }

        public int StatId { get; set; }
        public int TotalGoals { get; set; }
        public int FirstHalfGoals { get; set; }
        public int TotalCards { get; set; }
        public int FirstHalfCards { get; set; }

        public virtual ICollection<Match> MatchAwayTeamStats { get; set; }
        public virtual ICollection<Match> MatchHomeTeamStats { get; set; }
    }
}
