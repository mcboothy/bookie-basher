using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Team
    {
        public Team()
        {
            Averagestat = new HashSet<AverageStat>();
            MatchAwayTeam = new HashSet<Match>();
            MatchHomeTeam = new HashSet<Match>();
            TeamAlias = new HashSet<TeamAlias>();
        }

        public int TeamId { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public int? SeasonId { get; set; }

        public virtual Season Season { get; set; }
        public virtual ICollection<AverageStat> Averagestat { get; set; }
        public virtual ICollection<Match> MatchAwayTeam { get; set; }
        public virtual ICollection<Match> MatchHomeTeam { get; set; }
        public virtual ICollection<TeamAlias> TeamAlias { get; set; }
    }
}
