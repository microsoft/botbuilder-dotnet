// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Implements extension methods for language generator.
    /// </summary>
    public static class LanguageGeneratorExtensions
    {
        //------------Solution1------------------------
        private static ConditionalWeakTable<ResourceExplorer, LanguageGeneratorManager> languageGeneratorManagers = new ConditionalWeakTable<ResourceExplorer, LanguageGeneratorManager>();

        //---------------Solution2-------------------------//
        //private static ObjectCache cache = MemoryCache.Default;
        //
        // //Set the expireTime to 1 minutes.
        //private static readonly int expireMinutes = 1;

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

            var resourceExplorer = dialogManager.InitialTurnState.Get<ResourceExplorer>();

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
            var resourceExplorer = dialogManager.InitialTurnState.Get<ResourceExplorer>();

            //-------------------Solution1----------------------
            lock (languageGeneratorManagers)
            {
                if (!languageGeneratorManagers.TryGetValue(resourceExplorer ?? throw new InvalidOperationException($"Unable to get an instance of {nameof(resourceExplorer)}."), out var lgm))
                {
                    lgm = new LanguageGeneratorManager(resourceExplorer);
                    languageGeneratorManagers.Add(resourceExplorer, lgm);
                }

                dialogManager.InitialTurnState.Add<LanguageGeneratorManager>(lgm);
                dialogManager.InitialTurnState.Add<LanguageGenerator>(languageGenerator ?? throw new ArgumentNullException(nameof(languageGenerator)));

                return dialogManager;
            }

            //--------------------------------------------------

            //-------------------Solution2----------------------
            //LanguageGeneratorManager lgm;
            //var cacheItem = cache.Get(resourceExplorer.GetHashCode() + string.Empty);
            //if (cacheItem != null)
            //{
            //    lgm = new LanguageGeneratorManager(resourceExplorer);
            //    cache.Add(resourceExplorer.GetHashCode() + string.Empty, lgm, new CacheItemPolicy
            //    {
            //        AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(expireMinutes))
            //    });
            //}
            //else
            //{
            //    lgm = cacheItem as LanguageGeneratorManager;
            //}

            //dialogManager.InitialTurnState.Add<LanguageGeneratorManager>(lgm);
            //dialogManager.InitialTurnState.Add<LanguageGenerator>(languageGenerator ?? throw new ArgumentNullException(nameof(languageGenerator)));

            //return dialogManager;

            //--------------------------------------------------
        }

        /// <summary>
        /// Register language policy as default policy.
        /// </summary>
        /// <param name="dialogManager">botAdapter to add services to.</param>
        /// <param name="policy">policy to use.</param>
        /// <returns>botAdapter.</returns>
        public static DialogManager UseLanguagePolicy(this DialogManager dialogManager, LanguagePolicy policy)
        {
            dialogManager.InitialTurnState.Add<LanguagePolicy>(policy);
            return dialogManager;
        }
    }
}
