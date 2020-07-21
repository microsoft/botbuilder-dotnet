// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines health check result values.
    /// </summary>
    public class HealthResults
    {
        /// <summary>
        /// Gets or sets a value indicating whether the health check is success.
        /// </summary>
        [JsonProperty(PropertyName = "success")]
#pragma warning disable SA1609 // Property documentation should have value
        public bool Success { get; set; }
#pragma warning restore SA1609 // Property documentation should have value

        /// <summary>
        /// Gets or sets the authorization property of a health result.
        /// </summary>
        [JsonProperty(PropertyName = "authorization")]
#pragma warning disable SA1609 // Property documentation should have value
        public string Authorization { get; set; }
#pragma warning restore SA1609 // Property documentation should have value

        /// <summary>
        /// Gets or sets the user agent of a health result.
        /// </summary>
        [JsonProperty(PropertyName = "user-agent")]
#pragma warning disable SA1609 // Property documentation should have value
        public string UserAgent { get; set; }
#pragma warning restore SA1609 // Property documentation should have value

        /// <summary>
        /// Gets or sets the collection of messages of a health result.
        /// </summary>
        [JsonProperty(PropertyName = "messages")]
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat).
#pragma warning disable SA1609 // Property documentation should have value
        public string[] Messages { get; set; }
#pragma warning restore SA1609 // Property documentation should have value
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets or sets a diagnostics object for a health result.
        /// </summary>
        [JsonProperty(PropertyName = "diagnostics")]
#pragma warning disable SA1609 // Property documentation should have value
        public object Diagnostics { get; set; }
#pragma warning restore SA1609 // Property documentation should have value
    }
}
