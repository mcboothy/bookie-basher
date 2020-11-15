using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Season
    {
        public Season()
        {
            AverageStat = new HashSet<AverageStat>();
            Match = new HashSet<Match>();
            Team = new HashSet<Team>();
            UnknownTeams = new HashSet<UnknownTeams>();
        }

        public int SeasonId { get; set; }
        public int CompetitionId { get; set; }
        public string Year { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual Competition Competition { get; set; }
        public virtual ICollection<AverageStat> AverageStat { get; set; }
        public virtual ICollection<Match> Match { get; set; }
        public virtual ICollection<Team> Team { get; set; }
        public virtual ICollection<UnknownTeams> UnknownTeams { get; set; }
    }
}
