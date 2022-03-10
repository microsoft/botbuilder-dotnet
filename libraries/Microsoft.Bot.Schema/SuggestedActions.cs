// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// SuggestedActions that can be performed.
    /// </summary>
    public class SuggestedActions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestedActions"/> class.
        /// </summary>
        public SuggestedActions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestedActions"/> class.
        /// </summary>
        /// <param name="to">Ids of the recipients that the actions should be
        /// shown to.  These Ids are relative to the channelId and a subset of
        /// all recipients of the activity.</param>
        /// <param name="actions">Actions that can be shown to the user.</param>
        public SuggestedActions(IList<string> to = default, IList<CardAction> actions = default)
        {
            To = to ?? new List<string>();
            Actions = actions ?? new List<CardAction>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestedActions"/> class.
        /// </summary>
        /// <param name="to">Ids of the recipients that the actions should be
        /// shown to. These Ids are relative to the channelId and a subset of
        /// all recipients of the activity.</param>
        /// <param name="actions">Actions that can be shown to the user.</param>
        /// <exception cref="ArgumentNullException">ArgumentNullException.</exception>
        public SuggestedActions(IEnumerable<string> to, IEnumerable<CardAction> actions)
            : this(to.ToArray(), actions.ToArray())
        {
        }

        /// <summary>
        /// Gets ids of the recipients that the actions should be shown
        /// to.  These Ids are relative to the channelId and a subset of all
        /// recipients of the activity.
        /// </summary>
        /// <value>The ID's of the recipients that the actions should be shown to.</value>
        [JsonProperty(PropertyName = "to")]
        public IList<string> To { get; private set; } = new List<string>();

        /// <summary>
        /// Gets actions that can be shown to the user.
        /// </summary>
        /// <value>The actions that can be shown to the user.</value>
        [JsonProperty(PropertyName = "actions")]
        public IList<CardAction> Actions { get; private set; } = new List<CardAction>();
    }
}
