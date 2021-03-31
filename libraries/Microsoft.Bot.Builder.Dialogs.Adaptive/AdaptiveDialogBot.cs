// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// An <see cref="IBot"/> implementation that manages the execution of an <see cref="AdaptiveDialog"/>.
    /// </summary>
    public class AdaptiveDialogBot : IBot
    {
        private readonly BotFrameworkAuthentication _botFrameworkAuthentication;
        private readonly DialogManager _dialogManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveDialogBot"/> class.
        /// </summary>
        /// <param name="adaptiveDialogId">The id of the <see cref="AdaptiveDialog"/> to load from the <see cref="ResourceExplorer"/>.</param>
        /// <param name="languageGeneratorId">The id of the <see cref="LanguageGenerator"/> to load from the <see cref="ResourceExplorer"/>.</param>
        /// <param name="defaultLocale">The default locale for this bot.</param>
        /// <param name="resourceExplorer">The Bot Builder <see cref="ResourceExplorer"/> to load the <see cref="Dialog"/> from.</param>
        /// <param name="conversationState">A <see cref="ConversationState"/> implementation.</param>
        /// <param name="userState">A <see cref="UserState"/> implementation.</param>
        /// <param name="skillConversationIdFactoryBase">A <see cref="SkillConversationIdFactoryBase"/> implementation.</param>
        /// <param name="botFrameworkAuthentication">A <see cref="BotFrameworkAuthentication"/> used to obtain a client for making calls to Bot Builder Skills.</param>
        /// <param name="scopes">Custom <see cref="MemoryScope"/> implementations that extend the memory system.</param>
        /// <param name="pathResolvers">Custom <see cref="IPathResolver"/> that add new resolvers path shortcuts to memory scopes.</param>
        /// <param name="dialogs">Custom <see cref="Dialog"/> that will be added to the root DialogSet.</param>
        /// <param name="logger">An <see cref="ILogger"/> instance.</param>
        /// <param name="languagePolicy">Optional language policy.</param>
        public AdaptiveDialogBot(
            string adaptiveDialogId,
            string languageGeneratorId,
            string defaultLocale,
            ResourceExplorer resourceExplorer,
            ConversationState conversationState,
            UserState userState,
            SkillConversationIdFactoryBase skillConversationIdFactoryBase,
            BotFrameworkAuthentication botFrameworkAuthentication,
            IEnumerable<MemoryScope> scopes = default,
            IEnumerable<IPathResolver> pathResolvers = default,
            IEnumerable<Dialog> dialogs = default,
            ILogger logger = null,
            LanguagePolicy languagePolicy = null)
        {
            defaultLocale = defaultLocale ?? "en";

            if (adaptiveDialogId == null)
            {
                throw new ArgumentNullException(nameof(adaptiveDialogId));
            }

            if (resourceExplorer == null)
            {
                throw new ArgumentNullException(nameof(resourceExplorer));
            }

            this._botFrameworkAuthentication = botFrameworkAuthentication ?? throw new ArgumentNullException(nameof(botFrameworkAuthentication));

            this._dialogManager = new DialogManager();
            this._dialogManager.InitialTurnState.Add(resourceExplorer);
            this._dialogManager.InitialTurnState.Add(this._botFrameworkAuthentication);
            this._dialogManager.InitialTurnState.Add(logger ?? NullLogger<AdaptiveDialogBot>.Instance);
            this._dialogManager.InitialTurnState.Add(conversationState ?? throw new ArgumentNullException(nameof(conversationState)));
            this._dialogManager.InitialTurnState.Add(userState ?? throw new ArgumentNullException(nameof(userState)));
            this._dialogManager.InitialTurnState.Add(skillConversationIdFactoryBase ?? throw new ArgumentNullException(nameof(skillConversationIdFactoryBase)));
            this._dialogManager.InitialTurnState.Add(scopes ?? Enumerable.Empty<MemoryScope>());
            this._dialogManager.InitialTurnState.Add(pathResolvers ?? Enumerable.Empty<IPathResolver>());
            this._dialogManager.UseLanguageGeneration(languageGeneratorId);
            this._dialogManager.UseLanguagePolicy(languagePolicy ?? new LanguagePolicy(defaultLocale));

            this._dialogManager.RootDialog = resourceExplorer.LoadType<AdaptiveDialog>(adaptiveDialogId);

            if (dialogs != null)
            {
                foreach (var dialog in dialogs)
                {
                    this._dialogManager.Dialogs.Add(dialog);
                }
            }

            // put this on the TurnState using Set because some adapters (like BotFrameworkAdapter and CloudAdapter) will have already added it
            this._dialogManager.InitialTurnState.Set<BotCallbackHandler>(OnTurnAsync);
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            using (var botFrameworkClient = _botFrameworkAuthentication.CreateBotFrameworkClient())
            {
                turnContext.TurnState.Add(botFrameworkClient);

                // call dialog manager base class.
                await this._dialogManager.OnTurnAsync(turnContext, cancellationToken).ConfigureAwait(false);
                return;
            }
        }
    }
}
