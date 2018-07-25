// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Translation.Model
{
    public class TranslatedResult
    {
        [JsonProperty("translations")]
        public IEnumerable<TranslationModel> Translations { get; set; }
    }
}
