// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.Parsers.LU
{
    /// <summary>
    /// Class for diagnose and report errors.
    /// </summary>
    public class Diagnostic : Error
    {
        /// <summary>
        /// Raise an error.
        /// </summary>
        public const string ERROR = "ERROR";

        /// <summary>
        /// Raise a warning.
        /// </summary>
        public const string WARN = "WARN";

        /// <summary>
        /// Initializes a new instance of the <see cref="Diagnostic"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="severity">The exception severity.</param>
        /// <param name="range">The string range where the error was raised.</param>
        public Diagnostic(string message = null, string severity = "ERROR", Range range = null)
        {
            Range = range;
            Message = message;
            Severity = severity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Diagnostic"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="severity">The exception severity.</param>
        /// <param name="range">The string range where the error was raised.</param>
        /// <param name="context">The context of the error.</param>
        /// <returns>The <see cref="Diagnostic"/> instance.</returns>
        public static Diagnostic BuildDiagnostic(string message, string severity = ERROR, Range range = null, ParserRuleContext context = null)
        {
            Range actualRange = null;

            if (range != null)
            {
                var startPosition = new Position { Line = range.Start.Line, Character = range.Start.Character };
                var stopPosition = new Position { Line = range.End.Line, Character = range.End.Character };
                actualRange = new Range { Start = startPosition, End = stopPosition };
            }
            else if (context != null)
            {
                var startPosition = new Position { Line = context.Start.Line, Character = context.Start.Column };
                var stopPosition = new Position { Line = context.Stop.Line, Character = context.Stop.Column + context.Stop.Text.Length };
                actualRange = new Range { Start = startPosition, End = stopPosition };
            }

            return new Diagnostic(message, severity.ToString().ToUpperInvariant(), actualRange);
        }

        /// <summary>
        /// Returns the error report.
        /// </summary>
        /// <returns>The error report.</returns>
        public string StringMessage()
        {
            var secondMessage = Range == null ? Message : Range.StringMessage() + ":" + Message;
            return $"[{Severity}] {secondMessage}";
        }
    }
}
