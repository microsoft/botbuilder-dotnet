// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Object representing the operation state reponse.
    /// </summary>
    public class BatchOperationResponse
    {
        /// <summary>
        /// Gets the status map of the operation.
        /// </summary>
        /// <value>
        /// The status map for processed users.
        /// </value>
        [JsonProperty(PropertyName = "statusMap")]
        public IDictionary<string, int> StatusMap { get; } = new Dictionary<string, int>();
    }
}
