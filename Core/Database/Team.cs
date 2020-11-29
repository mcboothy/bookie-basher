using System;
using System.Collections.Generic;

#nullable disable

namespace BookieBasher.Core.Database
{
    public partial class Team
    {
        public Team()
        {
            AverageStats = new HashSet<AverageStat>();
            MatchAwayTeams = new HashSet<Match>();
            MatchHomeTeams = new HashSet<Match>();
            TeamAliases = new HashSet<TeamAlias>();
        }

        public int TeamId { get; set; }
        public string LogoUrl { get; set; }
        public int? SeasonId { get; set; }

        public virtual Season Season { get; set; }
        public virtual ICollection<AverageStat> AverageStats { get; set; }
        public virtual ICollection<Match> MatchAwayTeams { get; set; }
        public virtual ICollection<Match> MatchHomeTeams { get; set; }
        public virtual ICollection<TeamAlias> TeamAliases { get; set; }
    }
}
