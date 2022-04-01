using System;
using System.Collections.Generic;
using System.Text;

namespace Group
{
    public class Message
    {
        public object message { get; set; }
        public int statusCode { get; set; }

        public Message(object message, int statusCode)
        {
            this.message = message;
            this.statusCode = statusCode;
        }
    }
}
