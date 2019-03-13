// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Web.Http;
using Microsoft.Bot.Builder.Integration.AspNet.WebApi;

namespace Microsoft.Bot.Builder.TestBot.WebApi
{
    /// <summary>
    /// Documentation for BotConfig.
    /// </summary>
    public static class BotConfig
    {
        /// <summary>
        /// Joins a first name and a last name together into a single string.
        /// </summary>
        /// <param name="config">The Http Configuration.</param>
        public static void Register(HttpConfiguration config)
        {
            config.MapBotFramework(botConfig =>
            {
                botConfig.UseMicrosoftApplicationIdentity(null, null);

                // Uncomment these lines to debug with state

                //// The Memory Storage used here is for local bot debugging only. When the bot
                //// is restarted, everything stored in memory will be gone.
                // IStorage dataStore = new MemoryStorage();

                //// Create Conversation State object.
                //// The Conversation State object is where we persist anything at the conversation-scope.
                // var conversationState = new ConversationState(dataStore);
                // botConfig.BotFrameworkOptions.State.Add(conversationState);

                //// Create the custom state accessor.
                //// State accessors enable other components to read and write individual properties of state.
                // var accessors = new EchoBotAccessors(conversationState)
                // {
                //    CounterState = conversationState.CreateProperty<CounterState>(EchoBotAccessors.CounterStateName),
                // };

                // UnityConfig.Container.RegisterInstance<EchoBotAccessors>(accessors, new ContainerControlledLifetimeManager());
            });
        }
    }
}
