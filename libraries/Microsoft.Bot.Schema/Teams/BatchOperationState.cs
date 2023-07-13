// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Object representing operation state.
    /// </summary>
    public class BatchOperationState
    {
        /// <summary>
        /// Gets or sets the operation state.
        /// </summary>
        /// <value>
        /// The operation state.
        /// </value>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the operation state response.
        /// </summary>
        /// <value>
        /// The operation state response.
        /// </value>
        [JsonProperty(PropertyName = "response")]
        public BatchOperationResponse Response { get; set; }

        /// <summary>
        /// Gets or sets the total number of entries.
        /// </summary>
        /// <value>
        /// The number of entries.
        /// </value>
        [JsonProperty(PropertyName = "totalEntriesCount")]
        public int TotalEntriesCount { get; set; }
    }
}
