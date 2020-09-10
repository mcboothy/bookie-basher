using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class LeagueTeams
    {
        public int TeamId { get; set; }
        public int SeasonId { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
    }
}
