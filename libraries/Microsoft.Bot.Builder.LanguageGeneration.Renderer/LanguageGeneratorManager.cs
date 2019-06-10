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
                LanguageGenerators[resource.Id] = new TemplateEngineLanguageGenerator(resource.Id, resource.ReadText());
            }
            this.resourceExplorer.Changed += ResourceExplorer_Changed;
        }

        private void ResourceExplorer_Changed(IResource[] resources)
        {
            // reload changed LG files
            foreach (var resource in resources.Where(r => Path.GetExtension(r.Id).ToLower() == ".lg"))
            {
                LanguageGenerators[resource.Id] = new TemplateEngineLanguageGenerator(resource.Id, resource.ReadText());
            }
        }

        /// <summary>
        /// Generators
        /// </summary>
        public ConcurrentDictionary<string, ILanguageGenerator> LanguageGenerators { get; set; } = new ConcurrentDictionary<string, ILanguageGenerator>(StringComparer.OrdinalIgnoreCase);
    }
}
