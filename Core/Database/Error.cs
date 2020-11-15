using System;
using System.Collections.Generic;

namespace BookieBasher.Core.Database
{
    public partial class Error
    {
        public int ErrorId { get; set; }
        public string Request { get; set; }
        public string Error1 { get; set; }
    }
}
