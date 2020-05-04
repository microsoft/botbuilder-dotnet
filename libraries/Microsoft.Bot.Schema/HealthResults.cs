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
        public string[] Messages { get; set; }

        [JsonProperty(PropertyName = "diagnostics")]
        public object Diagnostics { get; set; }
    }
}
