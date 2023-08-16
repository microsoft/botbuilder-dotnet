// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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
        /// Gets the status map of the operation.
        /// </summary>
        /// <value>
        /// The status map for processed users.
        /// </value>
        [JsonProperty(PropertyName = "statusMap")]
        public IDictionary<int, int> StatusMap { get; } = new Dictionary<int, int>();

        /// <summary>
        /// Gets or sets the datetime value to retry the operation.
        /// </summary>
        /// <value>
        /// The datetime value to retry the operation.
        /// </value>
        [JsonProperty(PropertyName = "retryAfter")]
        public DateTime? RetryAfter { get; set; }

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
