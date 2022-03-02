﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Messaging extension Actions (Only when type is auth or config).
    /// </summary>
    public class MessagingExtensionSuggestedAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionSuggestedAction"/> class.
        /// </summary>
        /// <param name="actions">Actions.</param>
        public MessagingExtensionSuggestedAction(IList<CardAction> actions = default)
        {
            Actions = actions ?? new List<CardAction>();
        }

        /// <summary>
        /// Gets actions.
        /// </summary>
        /// <value>The actions.</value>
        [JsonProperty(PropertyName = "actions")]
        public IList<CardAction> Actions { get; private set; } = new List<CardAction>();
    }
}
