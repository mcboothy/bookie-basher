using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBasher.Core.Database
{
    public class GoalAverages
    {
        private Dictionary<string, AverageStat> statMap = new Dictionary<string, AverageStat>();

        public AverageStat this[string type]
        {
            get 
            {
                return statMap[type]; 
            }

            set 
            {
                statMap[type] = value;
            }
        }

        public GoalAverages(List<AverageStat> stats)
        {
            foreach(AverageStat stat in stats)
            {
                statMap.Add(stat.Type, stat);
            }
        }
    }
}
