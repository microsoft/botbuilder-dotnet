// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    public class HealthResults
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "authorization")]
        public string Authorization { get; set; }

        [JsonProperty(PropertyName = "user-agent")]
        public string UserAgent { get; set; }

        [JsonProperty(PropertyName = "messages")]
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat).
        public string[] Messages { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        [JsonProperty(PropertyName = "diagnostics")]
        public object Diagnostics { get; set; }
    }
}
