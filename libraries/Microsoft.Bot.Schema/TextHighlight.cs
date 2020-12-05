// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Refers to a substring of content within another field.
    /// </summary>
    public partial class TextHighlight
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextHighlight"/> class.
        /// </summary>
        public TextHighlight()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextHighlight"/> class.
        /// </summary>
        /// <param name="text">Defines the snippet of text to highlight.</param>
        /// <param name="occurrence">Occurrence of the text field within the
        /// referenced text, if multiple exist.</param>
        public TextHighlight(string text = default(string), int? occurrence = default(int?))
        {
            Text = text;
            Occurrence = occurrence;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets defines the snippet of text to highlight.
        /// </summary>
        /// <value>The snippet of text to highlight.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets occurrence of the text field within the referenced
        /// text, if multiple exist.
        /// </summary>
        /// <value>The number of occurrences of the text field within the referenced text.</value>
        [JsonProperty(PropertyName = "occurrence")]
        public int? Occurrence { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
