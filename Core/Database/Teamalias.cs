using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Teamalias
    {
        public int AliasId { get; set; }
        public int TeamId { get; set; }
        public string Alias { get; set; }
        public string Tag { get; set; }
        public sbyte IsDefault { get; set; }

        public virtual Team Team { get; set; }
    }
}
