using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="description">LG resource description.</param>
        /// <param name="id">The id of the lg source.</param>
        public LGResource(IEnumerable<LGTemplate> templates, string description, string id, IEnumerable<LGResource> imports = null)
        {
            Templates = templates;
            Imports = imports;
            Id = id;
            Description = description;
        }

        public string Description { get; set; }

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
        public IEnumerable<LGTemplate> Templates { get; set; }

        /// <summary>
        /// Gets or sets LgImports.
        /// </summary>
        /// <value>
        /// LG imports.
        /// </value>
        public IEnumerable<LGResource> Imports { get; set; }

        /// <summary>
        /// Override the Equals function for LGResource comparison.
        /// </summary>
        /// <param name="obj">The LGResource object.</param>
        /// <returns>True if the ids are same.</returns>
        public override bool Equals(object obj)
        {
            if (obj is LGResource lgresource)
            {
                return this.Id.Equals(lgresource.Id);
            }

            return false;
        }

        /// <summary>
        /// Override the GetHashCode function for LGResource comparison.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Discover all imported lg resources from a start resouce.
        /// </summary>
        /// <param name="start">The lg resource from which to start discovering imported resources.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <returns>LGResource list of parsed lg content.</returns>
        public List<LGResource> DiscoverLGResources()
        {
            var resourcesFound = new HashSet<LGResource>();
            ResolveImportResources(this, resourcesFound);

            return resourcesFound.ToList();
        }

        /// <summary>
        /// Resolve imported LG resources from a start resource.
        /// All the imports will be visited and resolved to LGResouce list.
        /// </summary>
        /// <param name="start">The lg resource from which to start resolving imported resources.</param>
        /// <param name="resourcesFound">Resources that have been found.</param>
        private void ResolveImportResources(LGResource start, HashSet<LGResource> resourcesFound)
        {
            resourcesFound.Add(start);

            foreach (var childResource in start.Imports)
            {
                try
                {
                    if (!resourcesFound.Contains(childResource))
                    {
                        ResolveImportResources(childResource, resourcesFound);
                    }
                }
                catch (Exception err)
                {
                    throw new Exception($"{childResource.Id}:{err.Message}", err);
                }
            }
        }
    }
}
