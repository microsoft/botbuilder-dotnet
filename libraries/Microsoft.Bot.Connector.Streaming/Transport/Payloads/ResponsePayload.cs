// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Streaming.Payloads;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Streaming.Payloads
{
    internal class ResponsePayload
    {
#pragma warning disable SA1609
        /// <summary>
        /// Gets or sets status - The Response Status.
        /// </summary>
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets assoicated stream descriptions.
        /// </summary>
        [JsonProperty("streams")]
        public List<StreamDescription> Streams { get; set; }
#pragma warning restore SA1609
    }
}
