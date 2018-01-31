// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Schema
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
