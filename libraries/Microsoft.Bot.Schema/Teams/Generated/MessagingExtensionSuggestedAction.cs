// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Messaging extension Actions (Only when type is auth or config)
    /// </summary>
    public partial class MessagingExtensionSuggestedAction
    {
        /// <summary>
        /// Initializes a new instance of the MessagingExtensionSuggestedAction
        /// class.
        /// </summary>
        public MessagingExtensionSuggestedAction()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MessagingExtensionSuggestedAction
        /// class.
        /// </summary>
        /// <param name="actions">Actions</param>
        public MessagingExtensionSuggestedAction(IList<CardAction> actions = default(IList<CardAction>))
        {
            Actions = actions;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets actions
        /// </summary>
        [JsonProperty(PropertyName = "actions")]
        public IList<CardAction> Actions { get; set; }

    }
}
