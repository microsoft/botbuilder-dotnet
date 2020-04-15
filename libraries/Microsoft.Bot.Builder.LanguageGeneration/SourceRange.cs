// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class SourceRange
    {
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
