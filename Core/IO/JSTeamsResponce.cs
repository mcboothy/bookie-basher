using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBasher.Core.IO
{
    public class JSCompetitionTeams
    {
        public JSSeason Season { get; set; }

        public List<JSTeam> WikiTeams { get; set; }

        public List<JSTeam> FSShortTeams { get; set; }

        public List<JSTeam> FSFullTeams { get; set; }
    }
}
