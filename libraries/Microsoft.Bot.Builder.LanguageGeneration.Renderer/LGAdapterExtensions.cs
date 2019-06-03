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
        /// <param name="botAdapter">BotAdapter</param>
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
            TypeFactory.Register("DefaultLanguageGenerator", typeof(ResourceMultiLanguageGenerator), new CustomLGLoader());
            botAdapter.Use(new RegisterClassMiddleware<LanguageGeneratorManager>(new LanguageGeneratorManager(resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer)))));

            // if there is no main.lg, then provide default engine (for inline expression evaluation only)
            if (resourceExplorer.GetResource(defaultLg) == null)
            {
                botAdapter.Use(new LanguageGeneratorMiddleware(new TemplateEngineLanguageGenerator(defaultLg, "")));
            }
            else
            {
                botAdapter.Use(new LanguageGeneratorMiddleware(new ResourceMultiLanguageGenerator(defaultLg)));
            }
            botAdapter.Use(new RegisterClassMiddleware<IMessageActivityGenerator>(messageGenerator ?? new TextMessageActivityGenerator()));
            return botAdapter;
        }

        /// <summary>
        /// Register ILanguageGenerator as default langugage generator
        /// </summary>
        /// <param name="botAdapter"></param>
        /// <param name="resourceExplorer"></param>
        /// <param name="languageGenerator"></param>
        /// <param name="messageGenerator"></param>
        /// <returns></returns>
        public static BotAdapter UseLanguageGeneration(this BotAdapter botAdapter, ResourceExplorer resourceExplorer, ILanguageGenerator languageGenerator, IMessageActivityGenerator messageGenerator = null)
        {
            TypeFactory.Register("DefaultLanguageGenerator", typeof(ResourceMultiLanguageGenerator), new CustomLGLoader());
            botAdapter.Use(new RegisterClassMiddleware<LanguageGeneratorManager>(new LanguageGeneratorManager(resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer)))));
            botAdapter.Use(new LanguageGeneratorMiddleware(languageGenerator ?? throw new ArgumentNullException(nameof(languageGenerator))));
            botAdapter.Use(new RegisterClassMiddleware<IMessageActivityGenerator>(messageGenerator ?? new TextMessageActivityGenerator()));
            return botAdapter;
        }
    }
}
