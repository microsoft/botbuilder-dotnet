// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// A range represents an ordered pair of two positions.
    /// </summary>
    public class Range
    {
        /// <summary>
        /// Default.
        /// </summary>
        public static readonly Range DefaultRange = new Range(1, 0, 1, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> class.
        /// </summary>
        /// <param name="start">Starting <see cref="Position"/> in a file.</param>
        /// <param name="end">Ending <see cref="Position"/> in a file.</param>
        public Range(Position start, Position end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> class.
        /// </summary>
        /// <param name="startLine">Starting line number in a file.</param>
        /// <param name="startChar">Starting character number in the start line.</param>
        /// <param name="endLine">Ending line number in a file.</param>
        /// <param name="endChar">Ending character number in the end line.</param>
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

        /// <inheritdoc/>
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
