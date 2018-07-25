// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Translation.Model
{
    public class Alignment
    {
        [JsonProperty("proj")]
        public string Projection { get; set; }
    }
}
