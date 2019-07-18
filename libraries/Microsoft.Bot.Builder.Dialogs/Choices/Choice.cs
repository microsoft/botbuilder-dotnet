// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Represents a choice for a choice prompt.
    /// </summary>
    public class Choice
    {
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
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the action to use when rendering the choice as a suggested action or hero card.
        /// This is optional.
        /// </summary>
        /// <value>
        /// The action to use when rendering the choice as a suggested action or hero card.
        /// </value>
        public CardAction Action { get; set; }

        /// <summary>
        /// Gets or sets the list of synonyms to recognize in addition to the value. This is optional.
        /// </summary>
        /// <value>
        /// The list of synonyms to recognize in addition to the value.
        /// </value>
        public List<string> Synonyms { get; set; }
    }
}
