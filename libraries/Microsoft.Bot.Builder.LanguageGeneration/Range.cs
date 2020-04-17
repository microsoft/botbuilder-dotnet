// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// A range represents an ordered pair of two positions.
    /// </summary>
    public class Range
    {
        public static readonly Range DefaultRange = new Range(1, 0, 1, 0);

        public Range(Position start, Position end)
        {
            Start = start;
            End = end;
        }

        public Range(int startLine, int startChar, int endLine, int endChar)
        {
            Start = new Position(startLine, startChar);
            End = new Position(endLine, endChar);
        }

        /// <summary>
        /// Gets or sets the start position. It is before or equal to <see cref="End"/>.
        /// </summary>
        /// <value>
        /// The start position. It is before or equal to <see cref="End"/>.
        /// </value>
        public Position Start { get; set; }

        /// <summary>
        /// Gets or sets the end position. It is after or equal to <see cref="Start"/>.
        /// </summary>
        /// <value>
        /// The end position. It is after or equal to <see cref="Start"/>.
        /// </value>
        public Position End { get; set; }

        public override string ToString()
        {
            var result = Start.ToString();
            if (Start.Line <= End.Line && Start.Character < End.Character)
            {
                result += $" - {End}";
            }

            return result;
        }
    }
}
