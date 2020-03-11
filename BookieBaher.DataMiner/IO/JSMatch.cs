using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBaher.DataMiner.IO
{
    public class JSMatch
    {
        public string MatchID { get; set; }

        public string FSMatchID { get; set; }

        public string DateTime { get; set; }

        public string HomeTeam { get; set; }

        public string AwayTeam { get; set; }

        public bool Postponed { get; set; }
    }
}
