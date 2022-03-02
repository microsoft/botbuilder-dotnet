// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Current user context, i.e., the current theme.
    /// </summary>
    public class TaskModuleRequestContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleRequestContext"/> class.
        /// </summary>
        public TaskModuleRequestContext()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleRequestContext"/> class.
        /// </summary>
        /// <param name="theme">The theme.</param>
        public TaskModuleRequestContext(string theme = default)
        {
            Theme = theme;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        /// <value>The theme.</value>
        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
