// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Represents a result from matching user input against a list of choices.
    /// </summary>
    public class FoundChoice
    {
        /// <summary>
        /// Gets or sets the value of the choice that was matched.
        /// </summary>
        /// <value>
        /// The value of the choice that was matched.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the choices index within the list of choices that was searched over.
        /// </summary>
        /// <value>
        /// The choices index within the list of choices that was searched over.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the accuracy with which the synonym matched the specified portion of the utterance. A
        /// value of 1.0 would indicate a perfect match.
        /// </summary>
        /// <value>
        /// The accuracy with which the synonym matched the specified portion of the utterance. A
        /// value of 1.0 would indicate a perfect match.
        /// </value>
        public float Score { get; set; }

        /// <summary>
        /// Gets or sets the synonym that was matched. This is optional.
        /// </summary>
        /// <value>
        /// The synonym that was matched.
        /// </value>
        public string Synonym { get; set; }
    }
}
