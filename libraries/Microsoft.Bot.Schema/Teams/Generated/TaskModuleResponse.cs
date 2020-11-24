﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Envelope for Task Module Response.
    /// </summary>
    public partial class TaskModuleResponse
    {
        /// <summary>
        /// Initializes a new instance of the TaskModuleResponse class.
        /// </summary>
        public TaskModuleResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TaskModuleResponse class.
        /// </summary>
        /// <param name="task">The JSON for the Adaptive card to appear in the
        /// task module.</param>
        public TaskModuleResponse(TaskModuleResponseBase task = default(TaskModuleResponseBase))
        {
            Task = task;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the JSON for the Adaptive card to appear in the task
        /// module.
        /// </summary>
        [JsonProperty(PropertyName = "task")]
        public TaskModuleResponseBase Task { get; set; }

        /// <summary>
        /// Gets or sets the CacheInfo for this TaskModuleResponse.
        /// module.
        /// </summary>
        [JsonProperty(PropertyName = "cacheInfo")]
        public CacheInfo CacheInfo { get; set; }
    }
}
