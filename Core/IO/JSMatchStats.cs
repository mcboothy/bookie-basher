using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBasher.Core.IO
{
    public class JSMatchStats
    {
        public JSTeamStats Home { get; set; }

        public JSTeamStats Away { get; set; }
    }

    public class JSTeamStats
    {
        public JSStats FirstHalf { get; set; }

        public JSStats SecondHalf { get; set; }
    }

    public class JSStats
    {
        public int Goals { get; set; }

        public int Cards { get; set; }
    }
}
