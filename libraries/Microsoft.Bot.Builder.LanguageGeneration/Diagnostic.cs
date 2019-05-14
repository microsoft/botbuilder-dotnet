using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
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

        public string Code { get; set; }

        public Range Range { get; set; }

        public DiagnosticSeverity Severity { get; set; }

        public string Source { get; set; }

        public string Message { get; }

        public override string ToString() => $"[{Severity}] {Range}: {Message}";
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

        public Position Start { get; set; }

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

        public int Line { get; set; }

        public int Character { get; set; }

        public override string ToString() => $"line {Line}:{Character}";
    }

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
}
