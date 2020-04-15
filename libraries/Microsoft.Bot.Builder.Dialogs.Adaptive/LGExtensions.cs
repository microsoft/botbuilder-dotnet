// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public static class LGExtensions
    {
        private static Dictionary<ResourceExplorer, LanguageGeneratorManager> languageGeneratorManagers = new Dictionary<ResourceExplorer, LanguageGeneratorManager>();

        /// <summary>
        /// Register default LG file as language generation.
        /// </summary>
        /// <param name="dialogManager">The <see cref="BotAdapter"/> to add services to.</param>
        /// <param name="defaultLg">Default LG Resource Id (default: main.lg).</param>
        /// <returns>The BotAdapter.</returns>
        public static DialogManager UseLanguageGeneration(
            this DialogManager dialogManager,
            string defaultLg = null)
        {
            if (defaultLg == null)
            {
                defaultLg = "main.lg";
            }

            var resourceExplorer = dialogManager.TurnState.Get<ResourceExplorer>();

            if (resourceExplorer.TryGetResource(defaultLg, out var resource))
            {
                dialogManager.UseLanguageGeneration(new ResourceMultiLanguageGenerator(defaultLg));
            }
            else
            {
                dialogManager.UseLanguageGeneration(new TemplateEngineLanguageGenerator());
            }

            return dialogManager;
        }

        /// <summary>
        /// Register ILanguageGenerator as default langugage generator.
        /// </summary>
        /// <param name="dialogManager">botAdapter to add services to.</param>
        /// <param name="languageGenerator">LanguageGenerator to use.</param>
        /// <returns>botAdapter.</returns>
        public static DialogManager UseLanguageGeneration(this DialogManager dialogManager, LanguageGenerator languageGenerator)
        {
            var resourceExplorer = dialogManager.TurnState.Get<ResourceExplorer>();

            lock (languageGeneratorManagers)
            {
                if (!languageGeneratorManagers.TryGetValue(resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer)), out var lgm))
                {
                    lgm = new LanguageGeneratorManager(resourceExplorer);
                    languageGeneratorManagers[resourceExplorer] = lgm;
                }

                dialogManager.TurnState.Add<LanguageGeneratorManager>(lgm);
                dialogManager.TurnState.Add<LanguageGenerator>(languageGenerator ?? throw new ArgumentNullException(nameof(languageGenerator)));

                return dialogManager;
            }
        }

        /// <summary>
        /// Register language policy as default policy.
        /// </summary>
        /// <param name="dialogManager">botAdapter to add services to.</param>
        /// <param name="policy">policy to use.</param>
        /// <returns>botAdapter.</returns>
        public static DialogManager UseLanguagePolicy(this DialogManager dialogManager, LanguagePolicy policy)
        {
            dialogManager.TurnState.Add<LanguagePolicy>(policy);
            return dialogManager;
        }
    }
}
