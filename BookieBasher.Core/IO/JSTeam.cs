using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBasher.Core.IO
{
    public class JSTeam
    {
        public string Name { get; set; }

        public int? SeasonId { get; set; }

        public string Url { get; set; }

        public List<JSAlias> Aliases { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
