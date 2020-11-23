using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBasher.Core.IO
{
    public class JSCompetitionTeams
    {
        public JSSeason Season { get; set; }

        public JSTeam[] WikiTeams { get; set; }

        public JSTeam[] FSShortTeams { get; set; }

        public JSTeam[] FSFullTeams { get; set; }
    }
}
