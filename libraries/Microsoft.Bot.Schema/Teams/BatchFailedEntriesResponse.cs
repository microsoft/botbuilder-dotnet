// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specifies the failed entries response.
    /// Contains a list of <see cref="BatchFailedEntry"/>.
    /// </summary>
    public class BatchFailedEntriesResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchFailedEntriesResponse"/> class.
        /// </summary>
        public BatchFailedEntriesResponse()
        {
        }

        /// <summary>
        /// Gets or sets the continuation token for paginated results.
        /// </summary>
        /// <value>The continuation token for paginated results.</value>
        [JsonProperty(PropertyName = "continuationToken")]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Gets the list of failed entries result of a batch operation.
        /// </summary>
        /// <value>The list of failed entries result of a batch operation.</value>
        [JsonProperty(PropertyName = "failedEntries")]
        public IList<BatchFailedEntry> FailedEntries { get; private set; } = new List<BatchFailedEntry>();
    }
}
