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

        /// <summary>
        /// Override the Equals function for LGResource comparison.
        /// </summary>
        /// <param name="obj">The LGResource object.</param>
        /// <returns>True if the ids are same.</returns>
        public override bool Equals(object obj)
        {
            if (obj is LGResource lgResourceObj)
            {
                return this.Id.Equals(lgResourceObj.Id);
            }

            return false;
        }

        /// <summary>
        /// Override the GetHashCode function for LGResource comparison.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
