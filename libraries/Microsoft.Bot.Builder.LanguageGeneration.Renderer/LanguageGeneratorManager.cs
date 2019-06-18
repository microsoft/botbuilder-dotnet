using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LanguageGeneratorManager
    {
        private ResourceExplorer resourceExplorer;

        public LanguageGeneratorManager(ResourceExplorer resourceExplorer)
        {
            this.resourceExplorer = resourceExplorer;
            foreach (var resource in this.resourceExplorer.GetResources("lg"))
            {
                LanguageGenerators[resource.Id] = new TemplateEngineLanguageGenerator(resource.ReadText(), importResolver: resourceResolver, name: resource.Id);
            }
            this.resourceExplorer.Changed += ResourceExplorer_Changed;
        }

        private void ResourceExplorer_Changed(IResource[] resources)
        {
            // reload changed LG files
            foreach (var resource in resources.Where(r => Path.GetExtension(r.Id).ToLower() == ".lg"))
            {
                LanguageGenerators[resource.Id] = new TemplateEngineLanguageGenerator(resource.ReadText(), importResolver: resourceResolver, name: resource.Id);
            }
        }

        /// <summary>
        /// Generators
        /// </summary>
        public ConcurrentDictionary<string, ILanguageGenerator> LanguageGenerators { get; set; } = new ConcurrentDictionary<string, ILanguageGenerator>(StringComparer.OrdinalIgnoreCase);

        private (string, string) resourceResolver(string id)
        {
            var res = resourceExplorer.GetResource(id);

            // If IResource is FileResource, use full name as the resource key to avoid duplicated imports 
            if (res is FileResource fileRes)
            {
                id = fileRes.FullName;
            }

            return ((res != null) ? res.ReadText() : string.Empty, id);
        }
    }
}
