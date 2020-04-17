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
        /// <param name="description">Import description, which is in [].</param>
        /// <param name="id">Import id, which is a path, in ().</param>
        /// <param name="sourceRange">Source range of template.</param>
        internal TemplateImport(string description, string id, SourceRange sourceRange)
        {
            this.SourceRange = sourceRange;
            this.Description = description;
            this.Id = id;
        }

        /// <summary>
        /// Gets or sets description of the import, included by '[]' in a lg file.
        /// </summary>
        /// <value>
        /// Description of the import, included by '[]' in a lg file.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets id of this import, included by '()' in a lg file.
        /// </summary>
        /// <value>
        /// Id of this import.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets original root source of the import.
        /// </summary>
        /// <value>
        /// Original root source of the import.
        /// </value>
        public SourceRange SourceRange { get; set; }

        public override string ToString()
        {
            return $"[{Description}]({Id})";
        }
    }
}
