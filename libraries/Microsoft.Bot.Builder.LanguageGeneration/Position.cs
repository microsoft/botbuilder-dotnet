// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Represents a line and character position, such as
    /// the position of the cursor.
    /// </summary>
    public class Position
    {
        public Position(int line, int character)
        {
            Line = line;
            Character = character;
        }

        /// <summary>
        /// Gets or sets the zero-based line value.
        /// </summary>
        /// <value>
        /// The zero-based line value.
        /// </value>
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the zero-based character value.
        /// </summary>
        /// <value>
        /// The zero-based character value.
        /// </value>
        public int Character { get; set; }

        public override string ToString() => $"line {Line}:{Character}";
    }
}
