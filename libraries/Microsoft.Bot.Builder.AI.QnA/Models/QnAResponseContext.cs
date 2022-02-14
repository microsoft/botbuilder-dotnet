﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// The context associated with QnA.  Used to mark if the qna response has related prompts to display.
    /// </summary>
    public class QnAResponseContext
    {
        /// <summary>
        /// Gets the prompts collection of related prompts.
        /// </summary>
        /// <value>
        /// The QnA prompts array.
        /// </value>
        [JsonProperty(PropertyName = "prompts")]
        public Collection<QnaMakerPrompt> Prompts { get; private set; } = new Collection<QnaMakerPrompt>();
    }
}
