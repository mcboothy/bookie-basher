using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Log
    {
        public int LogId { get; set; }
        public string ServiceName { get; set; }
        public string Message { get; set; }
        public string Host { get; set; }
    }
}
