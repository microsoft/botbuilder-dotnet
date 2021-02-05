// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Response of messaging extension action.
    /// </summary>
    public partial class MessagingExtensionActionResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionActionResponse"/> class.
        /// </summary>
        public MessagingExtensionActionResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionActionResponse"/> class.
        /// </summary>
        /// <param name="task">The JSON for the Adaptive card to appear in the
        /// task module.</param>
        /// <param name="composeExtension">A <see cref="MessagingExtensionResult"/> that initializes the current object's ComposeExension property.</param>
        public MessagingExtensionActionResponse(TaskModuleResponseBase task = default(TaskModuleResponseBase), MessagingExtensionResult composeExtension = default(MessagingExtensionResult))
        {
            Task = task;
            ComposeExtension = composeExtension;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the JSON for the Adaptive card to appear in the task
        /// module.
        /// </summary>
        /// <value>The JSON for the Adaptive card to appear in the task module.</value>
        [JsonProperty(PropertyName = "task")]
        public TaskModuleResponseBase Task { get; set; }

        /// <summary>
        /// Gets or sets the compose extension result.
        /// </summary>
        /// <value>The compose extension result.</value>
        [JsonProperty(PropertyName = "composeExtension")]
        public MessagingExtensionResult ComposeExtension { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CacheInfo "/> for this <see cref="MessagingExtensionActionResponse"/>.
        /// </summary>
        /// <value>The <see cref="CacheInfo "/> for this <see cref="MessagingExtensionActionResponse"/>.</value>
        [JsonProperty(PropertyName = "cacheInfo")]
        public CacheInfo CacheInfo { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
