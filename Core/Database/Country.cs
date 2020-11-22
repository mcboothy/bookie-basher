using System;
using System.Collections.Generic;

#nullable disable

namespace BookieBasher.Core.Database
{
    public partial class Country
    {
        public Country()
        {
            Competitions = new HashSet<Competition>();
        }

        public int CountryId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Competition> Competitions { get; set; }
    }
}
