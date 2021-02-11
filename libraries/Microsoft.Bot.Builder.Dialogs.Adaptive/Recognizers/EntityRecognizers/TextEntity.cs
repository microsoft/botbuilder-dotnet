// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Text entity base class.
    /// </summary>
    public class TextEntity : Entity
    {
        /// <summary>
        /// Result TypeName value.
        /// </summary>
        public const string TypeName = "text";

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEntity"/> class.
        /// </summary>
        public TextEntity()
            : base(TypeName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEntity"/> class.
        /// </summary>
        /// <param name="text">Text value.</param>
        public TextEntity(string text)
            : base(TypeName)
        {
            Text = text;
        }

        /// <summary>
        /// Gets or sets the text value.
        /// </summary>
        /// <value>
        /// Text value.
        /// </value>
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
