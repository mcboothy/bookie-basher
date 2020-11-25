using System;
using System.Collections.Generic;

#nullable disable

namespace BookieBasher.Core.Database
{
    public partial class Error
    {
        public int ErrorId { get; set; }
        public string ContentType { get; set; }
        public string Request { get; set; }
        public string Message { get; set; }
    }
}
