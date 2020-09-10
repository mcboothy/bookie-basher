using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Country
    {
        public Country()
        {
            Competition = new HashSet<Competition>();
        }

        public int CountryId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Competition> Competition { get; set; }
    }
}
