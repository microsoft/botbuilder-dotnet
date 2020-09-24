// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Strongly typed information corresponding to LUIS $instance value.
    /// </summary>
    public class InstanceData
    {
        /// <summary>
        /// Gets or sets 0-based index in the analyzed text for where entity starts.
        /// </summary>
        /// <value>
        /// 0-based index in the analyzed text for where entity starts.
        /// </value>
        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets 0-based index of the first character beyond the recognized entity.
        /// </summary>
        /// <value>
        /// 0-based index of the first character beyond the recognized entity.
        /// </value>
        [JsonProperty("endIndex")]
        public int EndIndex { get; set; }

        /// <summary>
        /// Gets or sets word broken and normalized text for the entity.
        /// </summary>
        /// <value>
        /// Word broken and normalized text for the entity.
        /// </value>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets optional confidence in the recognition.
        /// </summary>
        /// <value>
        /// Optional confidence in the recognition.
        /// </value>
        [JsonProperty("score")]
        public double? Score { get; set; }

        /// <summary>
        /// Gets or sets optional type for the entity.
        /// </summary>
        /// <value>
        /// Optional entity type.
        /// </value>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets optional subtype for the entity.
        /// </summary>
        /// <value>
        /// Optional entity subtype.
        /// </value>
        [JsonProperty("subtype")]
        public string Subtype { get; set; }

        /// <summary>
        /// Gets or sets any extra properties.
        /// </summary>
        /// <value>
        /// Any extra properties.
        /// </value>
        [JsonExtensionData(ReadData = true, WriteData = true)]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public IDictionary<string, object> Properties { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
