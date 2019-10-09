namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Here is a data model that can easily understanded and used as the LG import definition in lg files.
    /// </summary>
    public class LGImport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LGImport"/> class.
        /// </summary>
        /// <param name="parseTree">The parse tree of this template.</param>
        /// <param name="source">Source of this import.</param>
        public LGImport(LGFileParser.ImportDefinitionContext parseTree, string source = "")
        {
            ParseTree = parseTree;
            Source = source;

            Description = ExtractDescription(parseTree);
            Id = ExtractId(parseTree);
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

        /// <summary>
        /// Gets the parse tree of this lg file.
        /// </summary>
        /// <value>
        /// The parse tree of this lg file.
        /// </value>
        public LGFileParser.ImportDefinitionContext ParseTree { get; }

        private string ExtractDescription(LGFileParser.ImportDefinitionContext parseTree) => parseTree.IMPORT_DESC()?.GetText()?.Trim('[').Trim(']');

        private string ExtractId(LGFileParser.ImportDefinitionContext parseTree) => parseTree.IMPORT_PATH()?.GetText()?.Trim('(').Trim(')');
    }
}
