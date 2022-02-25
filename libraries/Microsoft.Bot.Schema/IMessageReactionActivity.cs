// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A reaction to a Message Activity.
    /// </summary>
    public interface IMessageReactionActivity : IActivity
    {
        /// <summary>
        /// Gets reactions added to the activity.
        /// </summary>
        /// <value>
        /// Reactions added to the activity.
        /// </value>
        IList<MessageReaction> ReactionsAdded { get; }

        /// <summary>
        /// Gets reactions removed from the activity.
        /// </summary>
        /// <value>
        /// Reactions removed from the activity.
        /// </value>
        IList<MessageReaction> ReactionsRemoved { get; }
    }
}
