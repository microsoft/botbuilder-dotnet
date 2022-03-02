// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Task module invoke request value payload.
    /// </summary>
    public class TaskModuleRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleRequest"/> class.
        /// </summary>
        public TaskModuleRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleRequest"/> class.
        /// </summary>
        /// <param name="data">User input data. Free payload with key-value pairs.</param>
        /// <param name="context">Current user context, i.e., the current theme.</param>
        public TaskModuleRequest(object data = default, TaskModuleRequestContext context = default)
        {
            Data = data;
            Context = context;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets user input data. Free payload with key-value pairs.
        /// </summary>
        /// <value>The user input data.</value>
        [JsonPropertyName("data")]
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets current user context, i.e., the current theme.
        /// </summary>
        /// <value>The current user context.</value>
        [JsonPropertyName("context")]
        public TaskModuleRequestContext Context { get; set; }

        /// <summary>
        /// Gets or sets current tab request context.
        /// </summary>
        /// <value>
        /// Tab request context.
        /// </value>
        [JsonPropertyName("tabContext")]
        public TabEntityContext TabEntityContext { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
