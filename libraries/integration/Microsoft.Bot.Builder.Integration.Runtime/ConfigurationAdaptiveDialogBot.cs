// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Integration.Runtime
{
    internal class ConfigurationAdaptiveDialogBot : AdaptiveDialogBot
    {
        private const string DefaultLanguageGeneratorId = "main.lg";
        private const string DefaultLocale = "en-US";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationAdaptiveDialogBot"/> class using <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <param name="resourceExplorer">The Bot Builder <see cref="ResourceExplorer"/> to load the <see cref="AdaptiveDialog"/> from.</param>
        /// <param name="conversationState">The <see cref="ConversationState"/> implementation to use for this <see cref="AdaptiveDialog"/>.</param>
        /// <param name="userState">The <see cref="UserState"/> implementation to use for this <see cref="AdaptiveDialog"/>.</param>
        /// <param name="skillConversationIdFactoryBase">The <see cref="SkillConversationIdFactoryBase"/> implementation to use for this <see cref="AdaptiveDialog"/>.</param>
        /// <param name="botFrameworkAuthentication">A <see cref="BotFrameworkAuthentication"/> for making calls to Bot Builder Skills.</param>
        /// <param name="logger">An <see cref="ILogger"/> instance.</param>
        public ConfigurationAdaptiveDialogBot(
            IConfiguration configuration,
            ResourceExplorer resourceExplorer,
            ConversationState conversationState,
            UserState userState,
            SkillConversationIdFactoryBase skillConversationIdFactoryBase,
            BotFrameworkAuthentication botFrameworkAuthentication = null,
            ILogger logger = null)
            : base(
                configuration.GetSection(ConfigurationConstants.RootDialogKey).Value,
                configuration.GetSection(ConfigurationConstants.LanguageGeneratorKey).Value ?? DefaultLanguageGeneratorId,
                configuration.GetSection(ConfigurationConstants.DefaultLocaleKey).Value ?? DefaultLocale,
                resourceExplorer,
                conversationState,
                userState,
                skillConversationIdFactoryBase,
                botFrameworkAuthentication ?? BotFrameworkAuthenticationFactory.Create(),
                logger: logger ?? NullLogger<AdaptiveDialogBot>.Instance)
        {
        }
    }
}
