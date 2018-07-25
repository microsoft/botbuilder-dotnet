// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Translation.Model
{
    public class ErrorModel
    {
        [JsonProperty("error")]
        public ErrorMessage Error { get; set; }
    }
}
