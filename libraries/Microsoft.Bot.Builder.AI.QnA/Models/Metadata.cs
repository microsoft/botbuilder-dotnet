// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Represents the Metadata object sent as part of QnA Maker requests.
    /// </summary>
    [Serializable]
    public class Metadata
    {
        /// <summary>
        /// Gets or sets the name for the Metadata property.
        /// </summary>
        /// <value>A string.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value for the Metadata property.
        /// </summary>
        /// <value>A string.</value>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
