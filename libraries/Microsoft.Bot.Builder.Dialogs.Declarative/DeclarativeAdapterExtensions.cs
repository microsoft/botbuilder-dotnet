using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public static class DeclarativeAdapterExtensions
    {
        /// <summary>
        /// Register ResourceExplorer and optionally register more types.
        /// </summary>
        /// <param name="botAdapter">BotAdapter to add middleware to.</param>
        /// <param name="resourceExplorer">resourceExplorer to use.</param>
        /// <param name="registerCustomTypes">function to add custom types.</param>
        /// <returns>The bot adapter.</returns>
        public static BotAdapter UseResourceExplorer(this BotAdapter botAdapter, ResourceExplorer resourceExplorer, Action registerCustomTypes = null)
        {
            TypeFactory.RegisterAdaptiveTypes();

            if (resourceExplorer == null)
            {
                throw new ArgumentNullException(nameof(resourceExplorer));
            }

            if (registerCustomTypes != null)
            {
                registerCustomTypes();
            }

            return botAdapter.Use(new RegisterClassMiddleware<ResourceExplorer>(resourceExplorer));
        }
    }
}
