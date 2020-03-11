using System;
using System.Collections.Generic;
using System.Text;

namespace BookieBasher.Core
{
    public class Message
    {
        public object Content { get; private set; }

        public string ReplyTo { get; private set; } = string.Empty;

        public bool IsPersistant { get; private set; } = true;

        public string ContentType { get; private set; } = "";

        public Dictionary<string, object> Headers { get; private set; }

        private Message()
        {

        }

        public static Message Create(object content, string type, string replyTo = "", bool isPersistant = true, Dictionary<string, object> headers = null)
        {
            return new Message()
            {
                Content = content,
                ContentType = type,
                IsPersistant = isPersistant,
                ReplyTo = replyTo,
                Headers = headers
            };
        }
    }
}
