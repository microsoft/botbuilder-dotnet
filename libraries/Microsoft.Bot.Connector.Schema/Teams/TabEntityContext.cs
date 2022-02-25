// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Current TabRequest entity context, or 'tabEntityId'.
    /// </summary>
    public partial class TabEntityContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabEntityContext"/> class.
        /// </summary>
        public TabEntityContext()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the entity id of the tab.
        /// </summary>
        /// <value>
        /// The entity id of the tab.
        /// </value>
        [JsonProperty(PropertyName = "tabEntityId")]
        public string TabEntityId { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
