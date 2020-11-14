using System;

namespace Microsoft.Bot.Builder.Exceptions
{
    /// <summary>
    /// A custom exception type for bot state errors.
    /// </summary>
    public class BotStateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotStateException"/> class.
        /// </summary>
        public BotStateException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotStateException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BotStateException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotStateException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference.</param>
        public BotStateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
