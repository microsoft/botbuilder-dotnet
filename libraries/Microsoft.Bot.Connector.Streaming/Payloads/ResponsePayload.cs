// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Streaming.Payloads
{
    internal class ResponsePayload
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("streams")]
        public List<StreamDescription> Streams { get; set; }
    }
}
