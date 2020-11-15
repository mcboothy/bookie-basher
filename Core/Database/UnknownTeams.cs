using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class UnknownTeams
    {
        public int Id { get; set; }
        public int SeasonId { get; set; }
        public string Request { get; set; }
        public string TeamsResponce { get; set; }

        public virtual Season Season { get; set; }
    }
}
