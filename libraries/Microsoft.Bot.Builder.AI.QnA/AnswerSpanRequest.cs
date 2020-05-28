// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    public class AnswerSpanRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the enablet.
        /// </summary>
        /// <value>
        /// The answer text.
        /// </value>
        [JsonProperty("enable")]
        public bool Enable { get; set; }
    }
}
