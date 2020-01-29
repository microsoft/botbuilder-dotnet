// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Declarative;

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
