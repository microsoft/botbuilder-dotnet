// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Streaming.Payloads
{
    internal class RequestPayload
    {
        [JsonProperty("verb")]
        public string Verb { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("streams")]
        public List<StreamDescription> Streams { get; set; }
    }
}
