// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A reaction to a Message Activity
    /// </summary>
    public class MessageReactionActivity : Activity
    {
        public MessageReactionActivity() : base(ActivityTypes.MessageReaction)
        {
        }

        /// <summary>
        /// Gets or sets reactions added to the activity
        /// </summary>
        [JsonProperty(PropertyName = "reactionsAdded")]
        public IList<MessageReaction> ReactionsAdded { get; set; }

        /// <summary>
        /// Gets or sets reactions removed from the activity
        /// </summary>
        [JsonProperty(PropertyName = "reactionsRemoved")]
        public IList<MessageReaction> ReactionsRemoved { get; set; }
    }
}
