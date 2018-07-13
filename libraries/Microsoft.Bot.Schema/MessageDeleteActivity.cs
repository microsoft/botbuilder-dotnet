using System;
using System.Linq;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Represents a request to delete a previous message activity in a conversation.
    /// </summary>
    public class MessageDeleteActivity : Activity
    {
        public static readonly MessageDeleteActivity Default = new MessageDeleteActivity();

        public MessageDeleteActivity() : base(ActivityTypes.MessageDelete)
        {
        }
    }
}

