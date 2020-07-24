// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Source range of the context. Including parse tree, source id and the context range.
    /// </summary>
    public class SourceRange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceRange"/> class.
        /// </summary>
        /// <param name="parseTree">A rule invocation record for parsing.</param>
        /// <param name="source">The source, used as the lg file path.</param>
        /// <param name="offset">The number of offset in parse tree.</param>
        public SourceRange(ParserRuleContext parseTree, string source = "", int offset = 0)
        {
            this.Source = source ?? string.Empty;
            this.Range = parseTree.ConvertToRange(offset);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceRange"/> class.
        /// </summary>
        /// <param name="range">The range of the block.</param>
        /// <param name="source">The source, used as the lg file path.</param>
        public SourceRange(Range range, string source = "")
        {
            this.Range = range;
            this.Source = source ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets range of the block.
        /// </summary>
        /// <value>
        /// range of the block.
        /// </value>
        public Range Range { get; set; }

        /// <summary>
        /// Gets or sets code source, used as the lg file path.
        /// </summary>
        /// <value>
        /// Code source, used as the lg file path.
        /// </value>
        public string Source { get; set; }
    }
}
