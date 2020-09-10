using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBasher.Core.Database
{
    public class GoalAverages
    {
        private Dictionary<string, Averagestat> statMap = new Dictionary<string, Averagestat>();

        public Averagestat this[string type]
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

        public GoalAverages(List<Averagestat> stats)
        {
            foreach(Averagestat stat in stats)
            {
                statMap.Add(stat.Type, stat);
            }
        }
    }
}
