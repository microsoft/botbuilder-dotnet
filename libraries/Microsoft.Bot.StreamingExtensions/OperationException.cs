using System;

namespace Microsoft.Bot.StreamingExtensions
{
    public class OperationException : Exception
    {
        public OperationException(string message, int statusCode, object body)
    : base(message)
        {
            StatusCode = statusCode;
            Body = body;
        }

        public int StatusCode { get; set; }

        public object Body { get; set; }
    }
}
