// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        /// <summary>
        /// Initializes a new instance of the <see cref="Diagnostic"/> class.
        /// </summary>
        /// <param name="range">Range where the error or warning occurred.</param>
        /// <param name="message">Error message of the error or warning.</param>
        /// <param name="severity">Severity of the error or warning.</param>
        /// <param name="source">Source of the error or warning occurred.</param>
        /// <param name="code">Code or identifier of the error or warning.</param>
        internal Diagnostic(
            Range range,
            string message,
            DiagnosticSeverity severity = DiagnosticSeverity.Error,
            string source = null,
            string code = null)
        {
            Message = message;
            Range = range;
            Severity = severity;
            Source = source;
            Code = code;
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
        /// Gets or sets a human-readable string describing the source of this diagnostic.
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

        /// <inheritdoc/>
        public override string ToString()
        {
            // ignore error range if source is inline content
            if (Source == TemplatesParser.InlineContentId)
            {
                return $"[{Severity}] {Source}: {Message}";
            }
            else
            {
                return $"[{Severity}] {Source} {Range}: {Message}";
            }
        }
    }
}
