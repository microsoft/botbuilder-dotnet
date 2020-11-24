// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Current user context, i.e., the current theme
    /// </summary>
    public partial class TaskModuleRequestContext
    {
        /// <summary>
        /// Initializes a new instance of the TaskModuleRequestContext class.
        /// </summary>
        public TaskModuleRequestContext()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TaskModuleRequestContext class.
        /// </summary>
        public TaskModuleRequestContext(string theme = default(string))
        {
            Theme = theme;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "theme")]
        public string Theme { get; set; }

    }
}
