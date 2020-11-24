// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Task module invoke request value payload
    /// </summary>
    public partial class TaskModuleRequest
    {
        /// <summary>
        /// Initializes a new instance of the TaskModuleRequest class.
        /// </summary>
        public TaskModuleRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TaskModuleRequest class.
        /// </summary>
        /// <param name="data">User input data. Free payload with key-value
        /// pairs.</param>
        /// <param name="context">Current user context, i.e., the current
        /// theme</param>
        public TaskModuleRequest(object data = default(object), TaskModuleRequestContext context = default(TaskModuleRequestContext))
        {
            Data = data;
            Context = context;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets user input data. Free payload with key-value pairs.
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets current user context, i.e., the current theme
        /// </summary>
        [JsonProperty(PropertyName = "context")]
        public TaskModuleRequestContext Context { get; set; }

    }
}
