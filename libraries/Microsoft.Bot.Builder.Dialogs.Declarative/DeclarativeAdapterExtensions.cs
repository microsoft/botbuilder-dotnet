using System;
using System.Collections.Generic;
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
        /// <param name="types">custom types to register.</param>
        /// <returns>The bot adapter.</returns>
        public static BotAdapter UseResourceExplorer(this BotAdapter botAdapter, ResourceExplorer resourceExplorer, IEnumerable<TypeRegistration> types = null)
        {
            if (resourceExplorer == null)
            {
                throw new ArgumentNullException(nameof(resourceExplorer));
            }

            DeclarativeTypeLoader.AddComponent(new DialogComponentRegistration());

            if (types != null)
            {
                foreach (var type in types)
                {
                    TypeFactory.Register(type.Name, type.Type, type.CustomDeserializer);
                }
            }

            return botAdapter.Use(new RegisterClassMiddleware<ResourceExplorer>(resourceExplorer));
        }
    }
}
