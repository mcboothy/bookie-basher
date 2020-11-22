using System;
using System.Collections.Generic;

#nullable disable

namespace BookieBasher.Core.Database
{
    public partial class Competition
    {
        public Competition()
        {
            CompetitionAliases = new HashSet<CompetitionAlias>();
            Seasons = new HashSet<Season>();
        }

        public int CompetitionId { get; set; }
        public int CountryId { get; set; }
        public string FlashScoreUrl { get; set; }
        public string Sponsor { get; set; }
        public string YearFounded { get; set; }
        public int? BetfairId { get; set; }
        public int? SoccerWikiId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual Country Country { get; set; }
        public virtual ICollection<CompetitionAlias> CompetitionAliases { get; set; }
        public virtual ICollection<Season> Seasons { get; set; }
    }
}
