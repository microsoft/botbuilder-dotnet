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
    public class AdaptiveDialogManager : DialogManager, IBot
    {
        private readonly string _adaptiveDialogId;
        private readonly BotFrameworkAuthentication _botFrameworkAuthentication;
        private readonly ResourceExplorer _resourceExplorer;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveDialogManager"/> class.
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
        public AdaptiveDialogManager(
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
            this._adaptiveDialogId = adaptiveDialogId ?? throw new ArgumentNullException(nameof(adaptiveDialogId));
            this._botFrameworkAuthentication = botFrameworkAuthentication ?? throw new ArgumentNullException(nameof(botFrameworkAuthentication));
            this._resourceExplorer = resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer));
            this._logger = logger ?? NullLogger<AdaptiveDialogManager>.Instance;
            this.InitialTurnState.Add(this._resourceExplorer);
            this.InitialTurnState.Add(this._botFrameworkAuthentication);
            this.InitialTurnState.Add(this._logger);
            this.InitialTurnState.Add(conversationState ?? throw new ArgumentNullException(nameof(conversationState)));
            this.InitialTurnState.Add(userState ?? throw new ArgumentNullException(nameof(userState)));
            this.InitialTurnState.Add(skillConversationIdFactoryBase ?? throw new ArgumentNullException(nameof(skillConversationIdFactoryBase)));
            this.InitialTurnState.Add(scopes ?? Enumerable.Empty<MemoryScope>());
            this.InitialTurnState.Add(pathResolvers ?? Enumerable.Empty<IPathResolver>());
            this.UseLanguageGeneration(languageGeneratorId);
            this.UseLanguagePolicy(languagePolicy ?? new LanguagePolicy(defaultLocale));

            if (dialogs != null)
            {
                foreach (var dialog in dialogs)
                {
                    this.Dialogs.Add(dialog);
                }
            }

            // put this on the TurnState using Set because some adapters (like BotFrameworkAdapter and CloudAdapter) will have already added it
            this.InitialTurnState.Set<BotCallbackHandler>(OnTurnAsync);
        }

        /// <inheritdoc/>
        public override async Task<DialogManagerResult> OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (this.RootDialog == null)
            {
                this.RootDialog = await _resourceExplorer.LoadTypeAsync<AdaptiveDialog>(_resourceExplorer.GetResource(this._adaptiveDialogId), cancellationToken).ConfigureAwait(false);
            }

            using (var botFrameworkClient = _botFrameworkAuthentication.CreateBotFrameworkClient())
            {
                turnContext.TurnState.Add(botFrameworkClient);

                // call dialog manager base class.
                return await base.OnTurnAsync(turnContext, cancellationToken).ConfigureAwait(false);
            }
        }

        Task IBot.OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return this.OnTurnAsync(turnContext, cancellationToken);
        }
    }
}
