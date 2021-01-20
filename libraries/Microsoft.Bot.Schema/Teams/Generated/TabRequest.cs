// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Invoke ('tab/fetch') request value payload.
    /// </summary>
    public partial class TabRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabRequest"/> class.
        /// </summary>
        public TabRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets current tab entity request context.
        /// </summary>
        /// <value>
        /// Tab context for this <see cref="TabRequest"/>.
        /// </value>
        [JsonProperty(PropertyName = "tabContext")]
        public TabEntityContext TabEntityContext { get; set; }

        /// <summary>
        /// Gets or sets current user context, i.e., the current theme.
        /// </summary>
        /// <value>
        /// Current user context, i.e., the current theme.
        /// </value>
        [JsonProperty(PropertyName = "context")]
        public TabContext Context { get; set; }

        /// <summary>
        /// Gets or sets state, which is the magic code for OAuth Flow.
        /// </summary>
        /// <value>
        /// State, which is the magic code for OAuth Flow.
        /// </value>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
