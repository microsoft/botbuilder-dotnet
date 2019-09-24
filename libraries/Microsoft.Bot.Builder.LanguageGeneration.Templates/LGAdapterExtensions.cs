using System;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration.Generators;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public static class LGAdapterExtensions
    {
        /// <summary>
        /// Register default LG file as language generation.
        /// </summary>
        /// <param name="botAdapter">The <see cref="BotAdapter"/> to add services to.</param>
        /// <param name="resourceExplorer">resource explorer to use for .lg based resources.</param>
        /// <param name="defaultLg">Default LG Resource Id (default: main.lg).</param>
        /// <returns>The BotAdapter.</returns>
        public static BotAdapter UseLanguageGeneration(
            this BotAdapter botAdapter,
            ResourceExplorer resourceExplorer = null,
            string defaultLg = null)
        {
            if (defaultLg == null)
            {
                defaultLg = "main.lg";
            }

            if (resourceExplorer == null)
            {
                resourceExplorer = new ResourceExplorer();
            }

            if (resourceExplorer.TryGetResource(defaultLg, out var resource))
            {
                botAdapter.UseLanguageGeneration(resourceExplorer, new ResourceMultiLanguageGenerator(defaultLg));
            }
            else
            {
                botAdapter.UseLanguageGeneration(resourceExplorer, new TemplateEngineLanguageGenerator(string.Empty, defaultLg, LanguageGeneratorManager.ResourceResolver(resourceExplorer)));
            }

            return botAdapter;
        }

        /// <summary>
        /// Register ILanguageGenerator as default langugage generator.
        /// </summary>
        /// <param name="botAdapter">botAdapter to add services to.</param>
        /// <param name="resourceExplorer">resourceExporer to provide to LanguageGenerator.</param>
        /// <param name="languageGenerator">LanguageGenerator to use.</param>
        /// <param name="messageGenerator">(OPTIONAL) Default is TextMessageActivityGenerator(). </param>
        /// <returns>botAdapter.</returns>
        public static BotAdapter UseLanguageGeneration(this BotAdapter botAdapter, ResourceExplorer resourceExplorer, ILanguageGenerator languageGenerator, IActivityGenerator messageGenerator = null)
        {
            DeclarativeTypeLoader.AddComponent(new LanguageGenerationComponentRegistration());
            botAdapter.Use(new RegisterClassMiddleware<LanguageGeneratorManager>(new LanguageGeneratorManager(resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer)))));
            botAdapter.Use(new RegisterClassMiddleware<ILanguageGenerator>(languageGenerator ?? throw new ArgumentNullException(nameof(languageGenerator))));
            botAdapter.UseMessageActivityGeneration(messageGenerator);
            return botAdapter;
        }

        /// <summary>
        /// Register MessageActivityGeneration. 
        /// </summary>
        /// <param name="botAdapter">botAdapter to add services to.</param>
        /// <param name="messageGenerator">(OPTIONAL) Default is TextMessageActivityGenerator(). </param>
        /// <returns>botAdapter.</returns>
        public static BotAdapter UseMessageActivityGeneration(this BotAdapter botAdapter, IActivityGenerator messageGenerator = null)
        {
            DeclarativeTypeLoader.AddComponent(new LanguageGenerationComponentRegistration());
            botAdapter.Use(new RegisterClassMiddleware<IActivityGenerator>(messageGenerator ?? new TextActivityGenerator()));
            return botAdapter;
        }
    }
}
