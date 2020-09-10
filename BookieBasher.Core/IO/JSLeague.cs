using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBasher.Core.IO
{
    public class JSLeague
    {
        public string Type { get; set; }

        public List<JSLeaguePosition> LeaguePositions { get; set; }
    }

    public class JSLeaguePosition
    {
        public short Position { get; set; }

        public string TeamName { get; set; }

        public short MatchesPlayed { get; set; }

        public short MatchesWon { get; set; }

        public short MatchesDrawn { get; set; }

        public short MatchesLost { get; set; }

        public string GoalsForAgains { get; set; }

        public short Points { get; set; }
    }
}
