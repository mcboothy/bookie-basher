using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Leagueteams
    {
        public int TeamId { get; set; }
        public int SeasonId { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
    }
}
