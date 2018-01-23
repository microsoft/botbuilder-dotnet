using System;
using System.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Conversation is ending, or a request to end the conversation
    /// </summary>
    public interface IEndOfConversationActivity : IActivity
    {
        /// <summary>
        /// Code indicating why the conversation has ended
        /// </summary>
        string Code { get; set; }

        /// <summary>
        /// Content to display when ending the conversation
        /// </summary>
        string Text { get; set; }
    }
}
