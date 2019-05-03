// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Tests.Utils.Luis
{
    public class LuisTestItem
    {
        [JsonProperty("text")]
        public string Utterance { get; set; }

        [JsonProperty("intent")]
        public string ExpectedIntent { get; set; }

        [JsonProperty("entities")]
        public LuisTestEntity[] ExpectedEntities { get; set; }
    }
}
