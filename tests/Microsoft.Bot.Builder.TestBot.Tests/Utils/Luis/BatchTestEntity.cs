// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Tests.Utils.Luis
{
    public class BatchTestEntity
    {
        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("startPos")]
        public int StartPos { get; set; }

        [JsonProperty("endPos")]
        public int EndPos { get; set; }
    }
}
