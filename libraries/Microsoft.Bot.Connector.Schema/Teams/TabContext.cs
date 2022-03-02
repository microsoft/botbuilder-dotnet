// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Current tab request context, i.e., the current theme.
    /// </summary>
    public partial class TabContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabContext"/> class.
        /// </summary>
        public TabContext()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the current user's theme.
        /// </summary>
        /// <value>
        /// The current user's theme.
        /// </value>
        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
