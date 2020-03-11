using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Season
    {
        public Season()
        {
            Averagestat = new HashSet<Averagestat>();
            Leagueposition = new HashSet<Leagueposition>();
            Match = new HashSet<Match>();
        }

        public int SeasonId { get; set; }
        public int CompetitionId { get; set; }
        public string Year { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual Competition Competition { get; set; }
        public virtual ICollection<Averagestat> Averagestat { get; set; }
        public virtual ICollection<Leagueposition> Leagueposition { get; set; }
        public virtual ICollection<Match> Match { get; set; }
    }
}
