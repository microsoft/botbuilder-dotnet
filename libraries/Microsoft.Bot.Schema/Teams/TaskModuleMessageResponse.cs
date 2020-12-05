// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    
    /// <summary>
    /// Task Module response with message action.
    /// </summary>
    public partial class TaskModuleMessageResponse : TaskModuleResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleMessageResponse"/> class.
        /// </summary>
        public TaskModuleMessageResponse()
            : base("message")
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleMessageResponse"/> class.
        /// </summary>
        /// <param name="value">Teams will display the value of value in a
        /// popup message box.</param>
        public TaskModuleMessageResponse(string value = default(string))
            : base("message")
        {
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets Teams will display the value of value in a popup
        /// message box.
        /// </summary>
        /// <value>The value Teams will display in a pop-up message box.</value>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
