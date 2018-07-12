// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Ai.Luis
{
    /// <summary>
    /// Strongly typed information corresponding to LUIS $instance value.
    /// </summary>
    public class InstanceData
    {
        /// <summary>
        /// 0-based index in the analyzed text for where entity starts.
        /// </summary>
        [JsonProperty("startIndex")]
        public int StartIndex;

        /// <summary>
        /// 0-based index of the first character beyond the recognized entity.
        /// </summary>
        [JsonProperty("endIndex")]
        public int EndIndex;

        /// <summary>
        /// Word broken and normalized text for the entity.
        /// </summary>
        [JsonProperty("text")]
        public string Text;

        /// <summary>
        /// Optional confidence in the recognition.
        /// </summary>
        [JsonProperty("score")]
        public double? Score;

        /// <summary>
        /// Any extra properties.
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; }
    }
}
