using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// A reaction to a Message Activity
    /// </summary>
    public interface IMessageReactionActivity : IActivity
    {
        /// <summary>
        /// Reactions added to the activity
        /// </summary>
        IList<MessageReaction> ReactionsAdded { get; set; }

        /// <summary>
        /// Reactions removed from the activity
        /// </summary>
        IList<MessageReaction> ReactionsRemoved { get; set; }
    }
}
