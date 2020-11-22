using System;
using System.Collections.Generic;

#nullable disable

namespace BookieBasher.Core.Database
{
    public partial class Season
    {
        public Season()
        {
            AverageStats = new HashSet<AverageStat>();
            Matches = new HashSet<Match>();
            Teams = new HashSet<Team>();
            UnknownTeams = new HashSet<UnknownTeam>();
        }

        public int SeasonId { get; set; }
        public int CompetitionId { get; set; }
        public string Year { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual Competition Competition { get; set; }
        public virtual ICollection<AverageStat> AverageStats { get; set; }
        public virtual ICollection<Match> Matches { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
        public virtual ICollection<UnknownTeam> UnknownTeams { get; set; }
    }
}
