// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.SettingMocks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.UserTokenMocks;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    /// <summary>
    /// Adaptive Testing <see cref="BotComponent"/> definition.
    /// </summary>
    public class AdaptiveTestingBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Converters
            services.AddSingleton<JsonConverterFactory, InterfaceConverterFactory<TestAction>>();
            services.AddSingleton<JsonConverterFactory, InterfaceConverterFactory<HttpRequestMock>>();
            services.AddSingleton<JsonConverterFactory, InterfaceConverterFactory<SettingMock>>();
            services.AddSingleton<JsonConverterFactory, InterfaceConverterFactory<UserTokenMock>>();

            // Actions for within normal bot flow
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<AssertCondition>(AssertCondition.Kind));

            // Test script actions
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<TestScript>(TestScript.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<UserSays>(UserSays.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<UserTyping>(UserTyping.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<UserConversationUpdate>(UserConversationUpdate.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<UserActivity>(UserActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<UserDelay>(UserDelay.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<AssertReply>(AssertReply.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<AssertReplyOneOf>(AssertReplyOneOf.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<AssertReplyActivity>(AssertReplyActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<AssertNoActivity>(AssertNoActivity.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<MemoryAssertions>(MemoryAssertions.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<AssertTelemetryContains>(AssertTelemetryContains.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<HttpRequestSequenceMock>(HttpRequestSequenceMock.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<UserTokenBasicMock>(UserTokenBasicMock.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<SetProperties>(SetProperties.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<SettingStringMock>(SettingStringMock.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<CustomEvent>(CustomEvent.Kind));
        }
    }
}
