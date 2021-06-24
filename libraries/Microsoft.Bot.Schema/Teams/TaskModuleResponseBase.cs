// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Base class for Task Module responses.
    /// </summary>
    public partial class TaskModuleResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleResponseBase"/> class.
        /// </summary>
        public TaskModuleResponseBase()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleResponseBase"/> class.
        /// </summary>
        /// <param name="type">Choice of action options when responding to the task/submit message. Possible values include: 'message', 'continue'.</param>
        public TaskModuleResponseBase(string type = default)
        {
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets choice of action options when responding to the
        /// task/submit message. Possible values include: 'message', 'continue'.
        /// </summary>
        /// <value>The choice of action options when responding to the task/submit message.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
