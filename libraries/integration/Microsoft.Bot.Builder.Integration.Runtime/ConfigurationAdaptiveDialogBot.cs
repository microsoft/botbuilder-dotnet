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
        /// <param name="storage">The <see cref="IStorage"/> implementation to use for this <see cref="AdaptiveDialog"/>.</param>
        /// <param name="botFrameworkAuthentication">A <see cref="BotFrameworkAuthentication"/> for making calls to Bot Builder Skills.</param>
        /// <param name="logger">An <see cref="ILogger"/> instance.</param>
        public ConfigurationAdaptiveDialogBot(
            IConfiguration configuration,
            ResourceExplorer resourceExplorer,
            IStorage storage = null,
            BotFrameworkAuthentication botFrameworkAuthentication = null,
            ILogger logger = null)
            : base(
                configuration.GetSection(ConfigurationConstants.RootDialogKey).Value,
                configuration.GetSection(ConfigurationConstants.LanguageGeneratorKey).Value ?? DefaultLanguageGeneratorId,
                configuration.GetSection(ConfigurationConstants.DefaultLocaleKey).Value ?? DefaultLocale,
                resourceExplorer,
                new ConversationState(storage ?? new MemoryStorage()),
                new UserState(storage ?? new MemoryStorage()),
                new SkillConversationIdFactory(storage ?? new MemoryStorage()),
                botFrameworkAuthentication ?? BotFrameworkAuthenticationFactory.Create(),
                logger ?? NullLogger<AdaptiveDialogBot>.Instance)
        {
        }
    }
}
