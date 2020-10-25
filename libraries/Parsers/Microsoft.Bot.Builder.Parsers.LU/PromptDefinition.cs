// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Parsers.LU
{
    /// <summary>
    /// Class for PrompDefinitions.
    /// </summary>
    public class PromptDefinition
    {
        /// <summary>
        /// Gets or sets the display text.
        /// </summary>
        /// <value>
        /// The text to display. 
        /// </value>
        [JsonProperty("displayText")]
        public string DisplayText { get; set; }

        /// <summary>
        /// Gets or sets the linked question.
        /// </summary>
        /// <value>
        /// The linked question. 
        /// </value>
        [JsonProperty("linkedQuestion")]
        public string LinkedQuestion { get; set; }

        /// <summary>
        /// Gets or sets if the prompt is context only.
        /// </summary>
        /// <value>
        /// If it's context only. 
        /// </value>
        [JsonProperty("contextOnly")]
        public string ContextOnly { get; set; }
    }
}
