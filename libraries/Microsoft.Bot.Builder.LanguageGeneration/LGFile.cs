using System.Collections.Generic;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Here is a data model that can easily understanded and used as the LG file definition.
    /// </summary>
    public class LGFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LGFile"/> class.
        /// </summary>
        /// <param name="templates">The lg templates.</param>
        /// <param name="imports">The lg imports.</param>
        /// <param name="path">The path of the lg file.</param>
        public LGFile(IList<LGTemplate> templates, IList<LGImport> imports, string path = "")
        {
            Templates = templates;
            Imports = imports;
            Path = path;
        }

        /// <summary>
        /// Gets or sets path of this lg file.
        /// </summary>
        /// <value>
        /// Path of this lg file.
        /// </value>
        public string Path { get; set; }

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
