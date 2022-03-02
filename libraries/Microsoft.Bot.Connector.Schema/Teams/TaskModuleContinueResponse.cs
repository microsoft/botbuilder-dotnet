// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Task Module Response with continue action.
    /// </summary>
    public class TaskModuleContinueResponse : TaskModuleResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleContinueResponse"/> class.
        /// </summary>
        public TaskModuleContinueResponse()
            : base("continue")
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleContinueResponse"/> class.
        /// </summary>
        /// <param name="value">The JSON for the Adaptive card to appear in the task module.</param>
        public TaskModuleContinueResponse(TaskModuleTaskInfo value = default)
            : base("continue")
        {
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the JSON for the Adaptive card to appear in the task module.
        /// </summary>
        /// <value>The JSON for the adaptive card to appear in the task module.</value>
        [JsonPropertyName("value")]
        public TaskModuleTaskInfo Value { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
