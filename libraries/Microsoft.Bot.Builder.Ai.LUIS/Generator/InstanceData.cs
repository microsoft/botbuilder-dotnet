// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Strongly typed information corresponding to LUIS $instance value.
    /// </summary>
    /// <remarks>
    /// This is a partial class in order to support adding custom meta-data.
    /// </remarks>
    public class InstanceData
    {
        /// <summary>
        /// 0-based index in the analyzed text for where entity starts.
        /// </summary>
        [JsonProperty("startIndex")]
        public int StartIndex;

        /// <summary>
        /// 0-based index of the last character for recognized entity.
        /// </summary>
        [JsonProperty("endIndex")]
        public int EndIndex;

        /// <summary>
        /// Original source text for the entity.
        /// </summary>
        [JsonProperty("text")]
        public string Text;

        /// <summary>
        /// Optional confidence in the recognition.
        /// </summary>
        [JsonProperty("score")]
        public double? Score;
    }
}
