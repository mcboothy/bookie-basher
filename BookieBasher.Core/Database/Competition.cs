using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Competition
    {
        public Competition()
        {
            Season = new HashSet<Season>();
        }

        public int CompetitionId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public virtual ICollection<Season> Season { get; set; }
    }
}
