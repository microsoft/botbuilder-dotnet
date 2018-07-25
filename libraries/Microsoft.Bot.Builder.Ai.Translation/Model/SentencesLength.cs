// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Translation.Model
{
    public class SentencesLength
    {
        [JsonProperty("srcSentLen")]
        public IEnumerable<int> Source { get; set; }

        [JsonProperty("transSentLen")]
        public IEnumerable<int> Translation { get; set; }
    }
}
