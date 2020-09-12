using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Competition
    {
        public Competition()
        {
            Competitionalias = new HashSet<CompetitionAlias>();
            Season = new HashSet<Season>();
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
        public virtual ICollection<CompetitionAlias> Competitionalias { get; set; }
        public virtual ICollection<Season> Season { get; set; }
    }
}
