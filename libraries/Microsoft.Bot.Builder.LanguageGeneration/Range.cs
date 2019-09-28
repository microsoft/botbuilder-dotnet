namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// A range represents an ordered pair of two positions.
    /// </summary>
    public class Range
    {
        public Range(Position start, Position end)
        {
            Start = start;
            End = end;
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
                result += " - ";
                result += End.ToString();
            }

            return result;
        }
    }
}
