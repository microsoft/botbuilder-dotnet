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
        /// <param name="content">original lg content.</param>
        /// <param name="id">The id of the lg source.</param>
        public LGResource(IList<LGTemplate> templates, IList<LGImport> imports, string content, string id = "")
        {
            Templates = templates;
            Imports = imports;
            Id = id;
            Content = content;
        }

        public string Content { get; set; }

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
        /// <param name="templateName">origin template name. the only id of a template.</param>
        /// <param name="parameters">new params.</param>
        /// <param name="templateBody">new template body.</param>
        /// <returns>new LG resource.</returns>
        public LGResource UpdateTemplate(string templateName, List<string> parameters, string templateBody)
        {
            var template = Templates.FirstOrDefault(u => u.Name == templateName);
            if (template == null)
            {
                return this;
            }
            var templateNameLine = BuildTemplateNameLine(templateName, parameters);
            var newTemplateBody = ConvertTemplateBody(templateBody);
            var content = $"{templateNameLine}\r\n{newTemplateBody}";
            var startLine = template.ParseTree.Start.Line - 1;
            var stopLine = template.ParseTree.Stop.Line - 1;

            var newContent = ReplaceRangeContent(Content, startLine, stopLine, content);
            return LGParser.Parse(newContent, Id);
        }

        /// <summary>
        /// Add a new template and return LG resource.
        /// </summary>
        /// <param name="templateName">new template name.</param>
        /// <param name="parameters">new params.</param>
        /// <param name="templateBody">new  template body.</param>
        /// <returns>new lg resource.</returns>
        public LGResource AddTemplate(string templateName, List<string> parameters, string templateBody)
        {
            var template = Templates.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                throw new Exception($"template {templateName} already exists.");
            }
            var templateNameLine = BuildTemplateNameLine(templateName, parameters);
            var newTemplateBody = ConvertTemplateBody(templateBody);
            var newContent = $"{Content}\r\n{templateNameLine}\r\n{newTemplateBody}";
            return LGParser.Parse(newContent, Id);
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

            var newContent = ReplaceRangeContent(Content, startLine, stopLine, null);
            return LGParser.Parse(newContent, Id);
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

        public override string ToString() => Content;

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

        private string ReplaceRangeContent(string originString, int startLine, int stopLine, string replaceString)
        {
            var originList = originString.Split('\n');
            var destList = new List<string>();
            if (startLine < 0 || startLine > stopLine || originList.Length <= stopLine)
            {
                throw new Exception("index out of range.");
            }

            destList.AddRange(originList.Take(startLine));

            if (!string.IsNullOrEmpty(replaceString))
            {
                destList.Add(replaceString);
            }

            destList.AddRange(originList.Skip(stopLine + 1));

            return string.Join("\n", destList);
        }

        private string ConvertTemplateBody(string templateBody)
        {
            if (string.IsNullOrWhiteSpace(templateBody))
            {
                return string.Empty;
            }
            var replaceList = templateBody.Split('\n');

            return string.Join("\n", replaceList.Select(u => WrapTemplateBodyString(u)));
        }

        // we will warp '# abc' into '- #abc', to avoid adding additional template.
        private string WrapTemplateBodyString(string replaceItem) => replaceItem.TrimStart().StartsWith("#") ? $"- {replaceItem.TrimStart()}" : replaceItem;

        private string BuildTemplateNameLine(string templateName, List<string> parameters) => $"# {templateName}({string.Join(", ", parameters)})";
    }
}
