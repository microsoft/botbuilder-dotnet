// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Expression container with source range.
    /// </summary>
    public class ExpressionRef
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionRef"/> class.
        /// </summary>
        /// <param name="expression">Expression string.</param>
        /// <param name="sourceRange">Expression source range.</param>
        public ExpressionRef(string expression, SourceRange sourceRange)
        {
            this.Expression = expression;
            this.SourceRange = sourceRange;
        }

        /// <summary>
        /// Gets or sets expression string.
        /// </summary>
        /// <value>
        /// Expression string.
        /// </value>
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets expression source range.
        /// </summary>
        /// <value>
        /// Expression source range.
        /// </value>
        public SourceRange SourceRange { get; set; }

        /// <summary>
        /// Return the Expression string.
        /// </summary>
        /// <returns>To string method.</returns>
        public override string ToString()
        {
            return Expression;
        }

        /// <summary>
        /// Get the unique id of expression context.
        /// </summary>
        /// <returns>id string.</returns>
        public string GetId()
        {
            if (SourceRange == null)
            {
                throw new InvalidOperationException("Source range is empty.");
            }

            return SourceRange.Source + ":" + SourceRange.Range + ":" + Expression;
        }
    }
}
