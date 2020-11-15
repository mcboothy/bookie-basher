using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBasher.Core.IO
{
    public class JSRequestMatch
    {
        public string MatchId { get; set; }

        public long Id { get; set; } = -1;

        public string HomeTeam { get; set; }

        public string AwayTeam { get; set; }

        public int RetryCount { get; set; } = 0;
    }
}
