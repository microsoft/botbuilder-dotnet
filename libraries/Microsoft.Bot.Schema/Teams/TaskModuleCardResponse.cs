﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.s

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    
    /// <summary>
    /// Tab Response to 'task/submit' from a tab.
    /// </summary>
    public class TaskModuleCardResponse : TaskModuleResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleCardResponse"/> class.
        /// </summary>
        public TaskModuleCardResponse()
            : base("continue")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleCardResponse"/> class.
        /// </summary>
        /// <param name="value">The JSON for the Adaptive cards to appear in the tab.</param>
        public TaskModuleCardResponse(TabResponse value)
            : base("continue")
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the JSON for the Adaptive cards to appear in the tab.
        /// </summary>
        /// <value>
        /// The JSON for the Adaptive cards to appear in the tab.
        /// </value>
        [JsonProperty(PropertyName = "value")]
        public TabResponse Value { get; set; }
    }
}
