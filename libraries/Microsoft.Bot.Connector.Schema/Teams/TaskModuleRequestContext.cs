// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Current user context, i.e., the current theme.
    /// </summary>
    public partial class TaskModuleRequestContext
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
        [JsonProperty(PropertyName = "theme")]
        public string Theme { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
