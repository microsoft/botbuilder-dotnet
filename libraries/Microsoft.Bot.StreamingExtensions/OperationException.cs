using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.StreamingExtensions
{
    public class OperationException : Exception
    {
        public int StatusCode { get; set; }

        public object Body { get; set; }

        public OperationException(string message, int statusCode, object body) :
            base(message)
        {
            StatusCode = statusCode;
            Body = body;
        }
    }
}
