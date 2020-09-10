using System;
using System.Collections.Generic;

namespace BookieBasher.Core.IO
{
    public class JSAlias
    {
        public string Name { get; set; }

        public string Tag { get; set; }

        public short IsDefault { get; set; }
    }

    public class JSCompetition
    {
        public int CompetitionId { get; set; }
        public int CountryId { get; set; }
        public List<JSAlias> Names { get; set; }
        public string DefaultAlias { get; set; }
        public string FlashScoreUrl { get; set; }
        public string Sponsor { get; set; }
        public string YearFounded { get; set; }
        public int? BetfairId { get; set; }
        public int? SoccerWikiId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
