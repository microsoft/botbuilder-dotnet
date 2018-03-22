using System;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Core.State
{
    [Serializable]
    public class StateOptimisticConcurrencyViolationException : Exception
    {
        public StateOptimisticConcurrencyViolationException(string message) : base(message)
        {
        }

        public StateOptimisticConcurrencyViolationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StateOptimisticConcurrencyViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}