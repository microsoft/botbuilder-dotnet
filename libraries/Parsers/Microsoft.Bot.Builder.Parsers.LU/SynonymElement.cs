// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable CA2227 // Collection properties should be read only

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Parsers.LU
{
    /// <summary>
    /// Class for Synonyms in List Entities.
    /// </summary>
    public class SynonymElement
    {
        [JsonProperty("NormalizedValue", NullValueHandling = NullValueHandling.Ignore)]
        public string NormalizedValue { get; set; } = null;

        [JsonProperty("Synonyms", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Synonyms { get; set; } = new List<string>();
    }
}
