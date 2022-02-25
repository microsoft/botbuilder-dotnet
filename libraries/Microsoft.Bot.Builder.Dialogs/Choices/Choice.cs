﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Represents a choice for a choice prompt.
    /// </summary>
#pragma warning disable CA1724 // Namespace name conflict (we can't change this without breaking binary compat)
    public class Choice
#pragma warning restore CA1724 // Namespace name conflict
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Choice"/> class to add a choice to a choice prompt.
        /// </summary>
        /// <param name="value">The value to return when the choice is selected.</param>
        public Choice(string value = null)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value to return when selected.
        /// </summary>
        /// <value>
        /// The value to return when selected.
        /// </value>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the action to use when rendering the choice as a suggested action or hero card.
        /// This is optional.
        /// </summary>
        /// <value>
        /// The action to use when rendering the choice as a suggested action or hero card.
        /// </value>
        [JsonProperty("action")]
        public CardAction Action { get; set; }

        /// <summary>
        /// Gets the list of synonyms to recognize in addition to the value. This is optional.
        /// </summary>
        /// <value>
        /// The list of synonyms to recognize in addition to the value.
        /// </value>
        [JsonProperty("synonyms")]
        public List<string> Synonyms { get; private set; } = new List<string>();
    }
}
