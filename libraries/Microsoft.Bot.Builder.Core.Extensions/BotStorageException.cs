using System;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public class BotStorageException : Exception
    {
        public BotStorageException()
        {
        }

        public BotStorageException(string message)
            : base(message)
        {
        }

        public BotStorageException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}