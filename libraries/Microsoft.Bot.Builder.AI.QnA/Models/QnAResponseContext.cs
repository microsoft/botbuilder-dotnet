// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// The context associated with QnA.  Used to mark if the qna response has related prompts to display.
    /// </summary>
    public class QnAResponseContext
    {
        /// <summary>
        /// Gets or sets the prompts collection of related prompts.
        /// </summary>
        /// <value>
        /// The QnA prompts array.
        /// </value>
        [JsonProperty(PropertyName = "prompts")]
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public QnaMakerPrompt[] Prompts { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
