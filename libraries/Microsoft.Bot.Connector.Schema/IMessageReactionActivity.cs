// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// A reaction to a Message Activity.
    /// </summary>
    public interface IMessageReactionActivity : IActivity
    {
        /// <summary>
        /// Gets or sets reactions added to the activity.
        /// </summary>
        /// <value>
        /// Reactions added to the activity.
        /// </value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        IList<MessageReaction> ReactionsAdded { get; set; }

        /// <summary>
        /// Gets or sets reactions removed from the activity.
        /// </summary>
        /// <value>
        /// Reactions removed from the activity.
        /// </value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        IList<MessageReaction> ReactionsRemoved { get; set; }
    }
}
