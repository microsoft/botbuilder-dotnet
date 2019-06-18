using System.Collections.Generic;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Here is a data model that can easily understanded and used as the LG file definition.
    /// </summary>
    public class LGResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LGResource"/> class.
        /// </summary>
        /// <param name="templates">The lg templates.</param>
        /// <param name="imports">The lg imports.</param>
        /// <param name="id">The id of the lg source.</param>
        public LGResource(IList<LGTemplate> templates, IList<LGImport> imports, string id = "")
        {
            Templates = templates;
            Imports = imports;
            Id = id;
        }

        /// <summary>
        /// Gets or sets id of this lg source.
        /// </summary>
        /// <value>
        /// id of this lg source. For file, is full path. for inline text, is 'inline'.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets LgTemplates.
        /// </summary>
        /// <value>
        /// LG templates.
        /// </value>
        public IList<LGTemplate> Templates { get; set; }

        /// <summary>
        /// Gets or sets LgImports.
        /// </summary>
        /// <value>
        /// LG imports.
        /// </value>
        public IList<LGImport> Imports { get; set; }
    }
}
