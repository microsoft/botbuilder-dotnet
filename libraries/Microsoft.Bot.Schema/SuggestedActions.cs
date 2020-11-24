// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// SuggestedActions that can be performed
    /// </summary>
    public partial class SuggestedActions
    {
        /// <summary>
        /// Initializes a new instance of the SuggestedActions class.
        /// </summary>
        public SuggestedActions()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the SuggestedActions class.
        /// </summary>
        /// <param name="to">Ids of the recipients that the actions should be
        /// shown to.  These Ids are relative to the channelId and a subset of
        /// all recipients of the activity</param>
        /// <param name="actions">Actions that can be shown to the user</param>
        public SuggestedActions(IList<string> to = default(IList<string>), IList<CardAction> actions = default(IList<CardAction>))
        {
            To = to;
            Actions = actions;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets ids of the recipients that the actions should be shown
        /// to.  These Ids are relative to the channelId and a subset of all
        /// recipients of the activity
        /// </summary>
        [JsonProperty(PropertyName = "to")]
        public IList<string> To { get; set; }

        /// <summary>
        /// Gets or sets actions that can be shown to the user
        /// </summary>
        [JsonProperty(PropertyName = "actions")]
        public IList<CardAction> Actions { get; set; }

    }
}
