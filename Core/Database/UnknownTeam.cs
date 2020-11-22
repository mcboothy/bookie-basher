using System;
using System.Collections.Generic;

#nullable disable

namespace BookieBasher.Core.Database
{
    public partial class UnknownTeam
    {
        public int Id { get; set; }
        public int SeasonId { get; set; }
        public string Request { get; set; }
        public string TeamsResponce { get; set; }

        public virtual Season Season { get; set; }
    }
}
