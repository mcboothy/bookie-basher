using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Team
    {
        public Team()
        {
            Averagestat = new HashSet<Averagestat>();
            MatchAwayTeam = new HashSet<Match>();
            MatchHomeTeam = new HashSet<Match>();
            Teamalias = new HashSet<Teamalias>();
        }

        public int TeamId { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public int? SeasonId { get; set; }

        public virtual Season Season { get; set; }
        public virtual ICollection<Averagestat> Averagestat { get; set; }
        public virtual ICollection<Match> MatchAwayTeam { get; set; }
        public virtual ICollection<Match> MatchHomeTeam { get; set; }
        public virtual ICollection<Teamalias> Teamalias { get; set; }
    }
}
