namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Represents the severity of diagnostics.
    /// </summary>
    public enum DiagnosticSeverity
    {
        /// <summary>
        /// Catch Error info.
        /// </summary>
        Error,

        /// <summary>
        /// Catch Warning info.
        /// </summary>
        Warning,

        /// <summary>
        /// Something to inform about but not a problem.
        /// </summary>
        Information,

        /// <summary>
        /// Something to hint to a better way of doing it, like proposing
        /// a refactoring.
        /// </summary>
        Hint,
    }

    /// <summary>
    /// Error/Warning report when parsing/evaluating template/inlineText.
    /// </summary>
    public class Diagnostic
    {
        public Diagnostic(
            Range range,
            string message,
            DiagnosticSeverity severity = DiagnosticSeverity.Error)
        {
            Message = message;
            Range = range;
            Severity = severity;
        }

        /// <summary>
        ///  Gets or sets a code or identifier for this diagnostics.
        /// </summary>
        /// <value>
        /// A code or identifier for this diagnostics.
        /// </value>
        public string Code { get; set; }

        /// <summary>
        ///  Gets or sets the range to which this diagnostic applies.
        /// </summary>
        /// <value>
        /// The range to which this diagnostic applies.
        /// </value>
        public Range Range { get; set; }

        /// <summary>
        /// Gets or sets the severity, default is <see cref="DiagnosticSeverity.Error"/>.
        /// </summary>
        /// <value>
        /// The severity, default is <see cref="DiagnosticSeverity.Error"/>.
        /// </value>
        public DiagnosticSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets a human-readable string describing the source of this
        /// diagnostic, e.g. 'typescript' or 'super lint'.
        /// </summary>
        /// <value>
        /// A human-readable string describing the source.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets the human-readable message.
        /// </summary>
        /// <value>
        /// The human-readable message.
        /// </value>
        public string Message { get; }

        public override string ToString()
        {
            // ignore error range if source is "inline"
            if (Source == "inline")
            {
                return $"[{Severity}] {Message}";
            }
            else
            {
                return $"[{Severity}] {Range}: {Message}";
            }
        }
    }

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
