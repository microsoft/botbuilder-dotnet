// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.QnA
{
    [Serializable]
    public class Metadata
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
