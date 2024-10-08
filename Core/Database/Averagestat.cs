﻿using System;
using System.Collections.Generic;

#nullable disable

namespace BookieBasher.Core.Database
{
    public partial class AverageStat
    {
        public int AverageStatId { get; set; }
        public int TeamId { get; set; }
        public int SeasonId { get; set; }
        public int GamesPlayed { get; set; }
        public string Type { get; set; }
        public double FirstHalfGoal { get; set; }
        public double SecondHalfGoal { get; set; }
        public double TotalGoal { get; set; }
        public double FirstHalfCard { get; set; }
        public double SecondHalfCard { get; set; }
        public double TotalCard { get; set; }
        public double PointPercentage { get; set; }

        public virtual Season Season { get; set; }
        public virtual Team Team { get; set; }
    }
}
