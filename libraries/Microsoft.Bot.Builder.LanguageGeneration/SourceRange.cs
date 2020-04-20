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
        public SourceRange(ParserRuleContext parseTree, string source = "", int offset = 0)
        {
            this.Source = source ?? string.Empty;
            this.ParseTree = parseTree;
            this.Range = parseTree.ConvertToRange(offset);
        }

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

        /// <summary>
        /// Gets or sets content parse tree form LGFileParser.g4.
        /// </summary>
        /// <value>
        /// Content parse tree form LGFileParser.g4.
        /// </value>
        public ParserRuleContext ParseTree { get; set; }
    }
}
