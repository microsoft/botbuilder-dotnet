// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specifies the failed entry with its id and error.
    /// </summary>
    public class BatchFailedEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchFailedEntry"/> class.
        /// </summary>
        public BatchFailedEntry()
        {
        }

        /// <summary>
        /// Gets or sets the id of the failed entry.
        /// </summary>
        /// <value>The id of the failed entry.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the error of the failed entry.
        /// </summary>
        /// <value>The error of the failed entry.</value>
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}
