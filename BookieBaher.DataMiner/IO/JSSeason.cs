using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBaher.DataMiner.IO
{
    public class JSSeason
    {
        public int SeasonId { get; set; }
        public int CompetitionId { get; set; }
        public string Year { get; set; }
        public string Status { get; set; }
        public JSCompetition Competition { get; set; }
    }
}
