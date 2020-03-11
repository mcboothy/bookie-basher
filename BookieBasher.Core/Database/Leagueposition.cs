using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Leagueposition
    {
        public int LeaguePositionId { get; set; }
        public int SeasonId { get; set; }
        public string Type { get; set; }
        public int TeamId { get; set; }
        public short Position { get; set; }
        public short MatchesPlayed { get; set; }
        public short GolsScored { get; set; }
        public short GoalsConceeded { get; set; }
        public short Points { get; set; }
        public short MatchesWon { get; set; }
        public short MatchesDrawn { get; set; }
        public short MatchesLost { get; set; }
        public double PointAverage { get; set; }

        public virtual Season Season { get; set; }
        public virtual Team Team { get; set; }
    }
}
