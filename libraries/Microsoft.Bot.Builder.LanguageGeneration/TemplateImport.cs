// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Class which which does actual import definition.</summary>
    /// <remarks>
    /// Here is a data model that can help users understand and use the LG import definition in LG files easily. 
    /// </remarks>
    public class TemplateImport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateImport"/> class.
        /// </summary>
        /// <param name="importStr">import string.</param>
        /// <param name="source">Source of this import.</param>
        internal TemplateImport(string importStr, string source = "")
        {
            Source = source;

            Description = ExtractDescription(importStr);
            Id = ExtractId(importStr);
        }

        /// <summary>
        /// Gets description of the import, what's included by '[]' in a lg file.
        /// </summary>
        /// <value>
        /// Description of the import, what's included by '[]' in a lg file.
        /// </value>
        public string Description { get; }

        /// <summary>
        /// Gets id of this import, what's included by '()' in a lg file.
        /// </summary>
        /// <value>
        /// Id of this import.
        /// </value>
        public string Id { get; }

        /// <summary>
        /// Gets origin root source of the import.
        /// </summary>
        /// <value>
        /// origin root source of the import.
        /// </value>
        public string Source { get; }

        private string ExtractDescription(string importString)
        {
            // content: [xxx](yyy)
            var openSquareBracketIndex = importString.IndexOf('[');
            var closeSquareBracketIndex = importString.IndexOf(']');
            return importString.Substring(openSquareBracketIndex + 1, closeSquareBracketIndex - openSquareBracketIndex - 1);
        }

        private string ExtractId(string importString)
        {
            // content: [xxx](yyy)
            var lastOpenBracketIndex = importString.LastIndexOf('(');
            var lastCloseBracketIndex = importString.LastIndexOf(')');
            return importString.Substring(lastOpenBracketIndex + 1, lastCloseBracketIndex - lastOpenBracketIndex - 1);
        }
    }
}
