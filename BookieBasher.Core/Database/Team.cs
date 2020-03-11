using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Team
    {
        public Team()
        {
            Averagestat = new HashSet<Averagestat>();
            Leagueposition = new HashSet<Leagueposition>();
            MatchAwayTeam = new HashSet<Match>();
            MatchHomeTeam = new HashSet<Match>();
        }

        public int TeamId { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }

        public virtual ICollection<Averagestat> Averagestat { get; set; }
        public virtual ICollection<Leagueposition> Leagueposition { get; set; }
        public virtual ICollection<Match> MatchAwayTeam { get; set; }
        public virtual ICollection<Match> MatchHomeTeam { get; set; }
    }
}
