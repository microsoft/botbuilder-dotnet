// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Localization;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class StateLanguageResolverTests
    {
        public static IEnumerable<object[]> StateLanguageResolverTestData()
        {
            const string enUs = "en-us";
            const string esAR = "es-ar";
            const string esMx = "es-mx";

            // params: conversationLocale, userLocale, activityLocale, expectedLocale
            // expected result: conversation > user > activity
            // Empty and null
            yield return new object[] { string.Empty, string.Empty, string.Empty, Thread.CurrentThread.CurrentCulture.Name };
            yield return new object[] { null, null, null, Thread.CurrentThread.CurrentCulture.Name };

            // Just one of 3 present
            yield return new object[] { esAR, string.Empty, string.Empty, esAR };
            yield return new object[] { string.Empty, esAR, string.Empty, esAR };
            yield return new object[] { string.Empty, string.Empty, esAR, esAR };

            // Locale pair combinations
            yield return new object[] { esAR, enUs, null, esAR };
            yield return new object[] { esAR, null, enUs, esAR };
            yield return new object[] { null, esAR, enUs, esAR };

            // All combinations
            yield return new object[] { esAR, enUs, esMx, esAR };
        }

        [Theory]
        [MemberData(nameof(StateLanguageResolverTestData))]

        public void StateLanguageResolution(string conversationLocale, string userLocale, string activityLocale, string expectedLocale)
        {
            // Arrange
            if (expectedLocale == null)
            {
                expectedLocale = activityLocale;
            }

            var dc = BuildTestDialogContext(conversationLocale, userLocale, activityLocale);
            var resolver = new StateLocaleResolver();

            // Act
            var computedLocale = resolver.Resolve(dc);

            // Assert
            Assert.NotNull(computedLocale);
            Assert.Equal(new CultureInfo(expectedLocale).Name, computedLocale.Name);
        }

        private static DialogContext BuildTestDialogContext(string conversationLocale, string userLocale, string activityLocale, bool registerUserState = true)
        {
            const string conversationLocaleProperty = "conversation.locale";
            const string userLocaleProperty = "user.locale";

            var turnContext = new TurnContext(
                new TestAdapter(), 
                new Activity() 
                { 
                    Type = ActivityTypes.Message, 
                    Locale = activityLocale, 
                    ChannelId = Channels.Test, 
                    Conversation = new ConversationAccount() { Id = "convo1234" },
                    From = new ChannelAccount() { Id = "testUser" }
                });            
            var dc = new DialogContext(new DialogSet(), turnContext, new DialogState());
            dc.Context.TurnState.Set(new ConversationState(new MemoryStorage()));

            if (registerUserState)
            {
                dc.Context.TurnState.Set(new UserState(new MemoryStorage()));
            }

            var dialogStateManager = new DialogStateManager(dc);
            dialogStateManager.LoadAllScopesAsync().GetAwaiter().GetResult();
            dc.Context.TurnState.Add(dialogStateManager);

            if (registerUserState)
            {
                dc.Context.TurnState.Set(new UserState(new MemoryStorage()));
                dc.State.SetValue(userLocaleProperty, userLocale);
            }

            dc.State.SetValue(conversationLocaleProperty, conversationLocale);

            return dc;
        }
    }
}
