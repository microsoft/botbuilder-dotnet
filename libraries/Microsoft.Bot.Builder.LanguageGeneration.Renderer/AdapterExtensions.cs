using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs
{
    public static class LGAdapterExtensions
    {
        internal class CustomLGLoader : ICustomDeserializer
        {
            public object Load(JToken obj, JsonSerializer serializer, Type type)
            {
                return new ResourceMultiLanguageGenerator(obj.Value<string>());
            }
        }

        /// <summary>
        /// Register default LG file as language generation
        /// </summary>
        /// <param name="botAdapter">botAdapter to add services to</param>
        /// <param name="resourceExplorer">resource explorer</param>
        /// <param name="defaultLg">Default LG Resource Id (default: main.lg)</param>
        /// <param name="messageGenerator"></param>
        /// <returns></returns>
        public static BotAdapter UseLanguageGeneration(this BotAdapter botAdapter, ResourceExplorer resourceExplorer, string defaultLg = null, IMessageActivityGenerator messageGenerator = null)
        {
            if (defaultLg == null)
            {
                defaultLg = "main.lg";
            }

            // if there is no main.lg, then provide default engine (for inline expression evaluation only)
            if (resourceExplorer.GetResource(defaultLg) == null)
            {
                botAdapter.UseLanguageGeneration(resourceExplorer, new TemplateEngineLanguageGenerator("", defaultLg, LanguageGeneratorManager.ResourceResolver(resourceExplorer)));
            }
            else
            {
                botAdapter.UseLanguageGeneration(resourceExplorer, new ResourceMultiLanguageGenerator(defaultLg));
            }
            return botAdapter;
        }

        /// <summary>
        /// Register ILanguageGenerator as default langugage generator
        /// </summary>
        /// <param name="botAdapter">botAdapter to add services to</param>
        /// <param name="resourceExplorer">resourceExporer to provide to LanguageGenerator</param>
        /// <param name="languageGenerator">LanguageGenerator to use</param>
        /// <param name="messageGenerator">(OPTIONAL) Default is TextMessageActivityGenerator() </param>
        /// <returns>botAdapter</returns>
        public static BotAdapter UseLanguageGeneration(this BotAdapter botAdapter, ResourceExplorer resourceExplorer, ILanguageGenerator languageGenerator, IMessageActivityGenerator messageGenerator = null)
        {
            TypeFactory.Register("DefaultLanguageGenerator", typeof(ResourceMultiLanguageGenerator), new CustomLGLoader());
            botAdapter.Use(new RegisterClassMiddleware<LanguageGeneratorManager>(new LanguageGeneratorManager(resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer)))));
            botAdapter.Use(new RegisterClassMiddleware<ILanguageGenerator>(languageGenerator ?? throw new ArgumentNullException(nameof(languageGenerator))));
            botAdapter.UseMessageActivityGeneration(messageGenerator);
            return botAdapter;
        }

        /// <summary>
        /// Register MessageActivityGeneration 
        /// </summary>
        /// <param name="botAdapter">botAdapter to add services to</param>
        /// <param name="messageGenerator">(OPTIONAL) Default is TextMessageActivityGenerator() </param>
        /// <returns>botAdapter</returns>
        public static BotAdapter UseMessageActivityGeneration(this BotAdapter botAdapter, IMessageActivityGenerator messageGenerator = null)
        {
            botAdapter.Use(new RegisterClassMiddleware<IMessageActivityGenerator>(messageGenerator ?? new TextMessageActivityGenerator()));
            return botAdapter;
        }
    }
}
