using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public static class AdaptiveAdapterExtensions
    {
        /// <summary>
        /// Register ResourceExplorer and optionally register more types.
        /// </summary>
        /// <param name="botAdapter">BotAdapter to add middleware to.</param>
        /// <returns>The bot adapter.</returns>
        public static BotAdapter UseAdaptiveDialogs(this BotAdapter botAdapter)
        {
            DeclarativeTypeLoader.AddComponent(new AdaptiveComponentRegistration());
            return botAdapter;
        }
    }
}
