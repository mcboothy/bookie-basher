using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBasher.Core.IO
{
    public class JSError
    {
        public string Request { get; set; }

        public string ContentType { get; set; }

        public string Error { get; set; }
    }
}
