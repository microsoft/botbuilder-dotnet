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
        /// <param name="originalContent">original lg content.</param>
        /// <param name="id">The id of the lg source.</param>
        public LGResource(IList<LGTemplate> templates, IList<LGImport> imports, string originalContent, string id = "")
        {
            Templates = templates;
            Imports = imports;
            Id = id;
            OriginalContent = originalContent;
        }

        public string OriginalContent { get; set; }

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
        /// update an exist template.
        /// </summary>
        /// <param name="templateName">origin template name.</param>
        /// <param name="templateBody">new template body.</param>
        /// <returns>new LG resource.</returns>
        public LGResource UpdateTemplate(string templateName, string templateBody)
        {
            var template = Templates.FirstOrDefault(u => u.Name == templateName);
            if (template == null)
            {
                return this;
            }

            var startLine = template.ParseTree.templateBody().Start.Line - 1;
            var stopLine = template.ParseTree.templateBody().Stop.Line - 1;

            var currentContent = ReplaceContent(OriginalContent, startLine, stopLine, templateBody);
            return LGParser.Parse(currentContent);
        }

        /// <summary>
        /// Add a new template and return LG resource.
        /// </summary>
        /// <param name="templateName">new template name.</param>
        /// <param name="templateBody">new  template body.</param>
        /// <returns>new lg resource.</returns>
        public LGResource AddTemplate(string templateName, string templateBody)
        {
            var template = Templates.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                throw new Exception($"template {templateName} already exists.");
            }

            var currentContent = $"{OriginalContent}\r\n# {templateName}\r\n{templateBody}\r\n";
            return LGParser.Parse(currentContent);
        }

        /// <summary>
        /// Delete an exist template.
        /// </summary>
        /// <param name="templateName">which template should delete.</param>
        /// <returns>return the new lg resource.</returns>
        public LGResource DeleteTemplate(string templateName)
        {
            var template = Templates.FirstOrDefault(u => u.Name == templateName);
            if (template == null)
            {
                return this;
            }

            var startLine = template.ParseTree.Start.Line - 1;
            var stopLine = template.ParseTree.Stop.Line - 1;

            var currentContent = ReplaceContent(OriginalContent, startLine, stopLine, null);
            return LGParser.Parse(currentContent);
        }

        /// <summary>
        /// Override the Equals function for LGResource comparison.
        /// </summary>
        /// <param name="obj">The <see cref="LGResource"/> object.</param>
        /// <returns>True if the ids are same.</returns>
        public override bool Equals(object obj)
        {
            if (obj is LGResource lgresourceObj)
            {
                return this.Id.Equals(lgresourceObj.Id);
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

        /// <summary>
        /// Discover all imported lg resources from a start resouce.
        /// </summary>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <returns>LGResource list of parsed lg content.</returns>
        public List<LGResource> DiscoverDependencies(ImportResolverDelegate importResolver)
        {
            var resourcesFound = new HashSet<LGResource>();
            ResolveImportResources(this, importResolver ?? ImportResolver.FileResolver, resourcesFound);

            return resourcesFound.ToList();
        }

        /// <summary>
        /// Resolve imported LG resources from a start resource.
        /// All the imports will be visited and resolved to LGResouce list.
        /// </summary>
        /// <param name="start">The lg resource from which to start resolving imported resources.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <param name="resourcesFound">Resources that have been found.</param>
        private void ResolveImportResources(LGResource start, ImportResolverDelegate importResolver, HashSet<LGResource> resourcesFound)
        {
            var resourceIds = start.Imports.Select(lg => lg.Id).ToList();
            resourcesFound.Add(start);

            foreach (var id in resourceIds)
            {
                try
                {
                    var (content, path) = importResolver(start.Id, id);
                    var childResource = LGParser.Parse(content, path);
                    if (!resourcesFound.Contains(childResource))
                    {
                        ResolveImportResources(childResource, importResolver, resourcesFound);
                    }
                }
                catch (Exception err)
                {
                    throw new Exception($"[Error]{id}:{err.Message}", err);
                }
            }
        }

        private string ReplaceContent(string originString, int startIndex, int endIndex, string replaceString)
        {
            var originList = originString.Split('\n');
            var destList = new List<string>();
            if (startIndex > endIndex || originList.Length <= endIndex)
            {
                throw new Exception("index out of range.");
            }

            for (var i=0; i< startIndex; i++)
            {
                destList.Add(originList[i].Trim());
            }

            if (!string.IsNullOrEmpty(replaceString))
            {
                destList.Add(replaceString);
            }

            for (var i = endIndex + 1; i< originList.Length; i++)
            {
                destList.Add(originList[i].Trim());
            }

            return string.Join("\r\n", destList);
        }
    }
}
