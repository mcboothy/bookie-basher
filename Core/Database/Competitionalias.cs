using System;
using System.Collections.Generic;

#nullable disable

namespace BookieBasher.Core.Database
{
    public partial class CompetitionAlias
    {
        public int AliasId { get; set; }
        public int CompetitionId { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public sbyte IsDefault { get; set; }

        public virtual Competition Competition { get; set; }
    }
}
