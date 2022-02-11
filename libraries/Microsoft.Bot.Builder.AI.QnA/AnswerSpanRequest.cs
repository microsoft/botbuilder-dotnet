// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// AnswerSpanRequest - model to enable precise answer.
    /// </summary>
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
