// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
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
        private static readonly ConcurrentDictionary<ResourceExplorer, LanguageGeneratorManager> _languageGeneratorManagers = new ConcurrentDictionary<ResourceExplorer, LanguageGeneratorManager>();

        private readonly string _adaptiveDialogId;
        private readonly string _languageGeneratorId;
        private readonly ResourceExplorer _resourceExplorer;
        private readonly SkillConversationIdFactoryBase _skillConversationIdFactoryBase;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private readonly BotFrameworkAuthentication _botFrameworkAuthentication;
        private readonly IBotTelemetryClient _telemetryClient;
        private readonly LanguagePolicy _languagePolicy;
        private readonly IEnumerable<MemoryScope> _memoryScopes;
        private readonly IEnumerable<IPathResolver> _pathResolvers;
        private readonly IEnumerable<Dialog> _dialogs;
        private readonly ILogger _logger;

        private readonly Lazy<Task<Dialog>> _lazyRootDialog;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveDialogBot"/> class.
        /// </summary>
        /// <param name="adaptiveDialogId">The id of the <see cref="AdaptiveDialog"/> to load from the <see cref="ResourceExplorer"/>.</param>
        /// <param name="languageGeneratorId">The id of the <see cref="LanguageGenerator"/> to load from the <see cref="ResourceExplorer"/>.</param>
        /// <param name="resourceExplorer">The Bot Builder <see cref="ResourceExplorer"/> to load the <see cref="Dialog"/> from.</param>
        /// <param name="conversationState">A <see cref="ConversationState"/> implementation.</param>
        /// <param name="userState">A <see cref="UserState"/> implementation.</param>
        /// <param name="skillConversationIdFactoryBase">A <see cref="SkillConversationIdFactoryBase"/> implementation.</param>
        /// <param name="languagePolicy">A <see cref="LanguagePolicy"/> to use.</param>
        /// <param name="botFrameworkAuthentication">A <see cref="BotFrameworkAuthentication"/> used to obtain a client for making calls to Bot Builder Skills.</param>
        /// <param name="telemetryClient">A <see cref="IBotTelemetryClient"/> used to log bot telemetry events.</param>
        /// <param name="scopes">Custom <see cref="MemoryScope"/> implementations that extend the memory system.</param>
        /// <param name="pathResolvers">Custom <see cref="IPathResolver"/> that add new resolvers path shortcuts to memory scopes.</param>
        /// <param name="dialogs">Custom <see cref="Dialog"/> that will be added to the root DialogSet.</param>
        /// <param name="logger">An <see cref="ILogger"/> instance.</param>
        public AdaptiveDialogBot(
            string adaptiveDialogId,
            string languageGeneratorId,
            ResourceExplorer resourceExplorer,
            ConversationState conversationState,
            UserState userState,
            SkillConversationIdFactoryBase skillConversationIdFactoryBase,
            LanguagePolicy languagePolicy,
            BotFrameworkAuthentication botFrameworkAuthentication,
            IBotTelemetryClient telemetryClient,
            IEnumerable<MemoryScope> scopes = default,
            IEnumerable<IPathResolver> pathResolvers = default,
            IEnumerable<Dialog> dialogs = default,
            ILogger logger = null)
        {
            _resourceExplorer = resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer));
            _adaptiveDialogId = adaptiveDialogId ?? throw new ArgumentNullException(nameof(adaptiveDialogId));
            _languageGeneratorId = languageGeneratorId ?? throw new ArgumentNullException(nameof(languageGeneratorId));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _skillConversationIdFactoryBase = skillConversationIdFactoryBase ?? throw new ArgumentNullException(nameof(skillConversationIdFactoryBase));
            _languagePolicy = languagePolicy ?? throw new ArgumentNullException(nameof(languagePolicy));
            _botFrameworkAuthentication = botFrameworkAuthentication ?? throw new ArgumentNullException(nameof(botFrameworkAuthentication));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _memoryScopes = scopes ?? Enumerable.Empty<MemoryScope>();
            _pathResolvers = pathResolvers ?? Enumerable.Empty<IPathResolver>();
            _dialogs = dialogs ?? Enumerable.Empty<Dialog>();
            _logger = logger ?? NullLogger<AdaptiveDialogBot>.Instance;

            _lazyRootDialog = new Lazy<Task<Dialog>>(CreateDialogAsync);
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"IBot.OnTurnContext for AdaptiveDialog '{_adaptiveDialogId}'");

            using (var botFrameworkClient = _botFrameworkAuthentication.CreateBotFrameworkClient())
            {
                // Set up the TurnState the Dialog is expecting
                SetUpTurnState(turnContext, botFrameworkClient);

                // Load the Dialog from the ResourceExplorer - the actual load should only happen once
                var rootDialog = await _lazyRootDialog.Value.ConfigureAwait(false);

                // Run the Dialog
                await rootDialog.RunAsync(turnContext, turnContext.TurnState.Get<ConversationState>().CreateProperty<DialogState>("DialogState"), cancellationToken).ConfigureAwait(false);

                // Save any updates that have been made
                await turnContext.TurnState.Get<ConversationState>().SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
                await turnContext.TurnState.Get<UserState>().SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
            }
        }

        private void SetUpTurnState(ITurnContext turnContext, BotFrameworkClient botFrameworkClient)
        {
            turnContext.TurnState.Add(botFrameworkClient);
            turnContext.TurnState.Add(_skillConversationIdFactoryBase);
            turnContext.TurnState.Add(_conversationState);
            turnContext.TurnState.Add(_userState);
            turnContext.TurnState.Add(_resourceExplorer);
            turnContext.TurnState.Add(_memoryScopes);
            turnContext.TurnState.Add(_pathResolvers);
            turnContext.TurnState.Add(_resourceExplorer.TryGetResource(_languageGeneratorId, out var resource) ? (LanguageGenerator)new ResourceMultiLanguageGenerator(_languageGeneratorId) : new TemplateEngineLanguageGenerator());
            turnContext.TurnState.Add(_languageGeneratorManagers.GetOrAdd(_resourceExplorer, _ => new LanguageGeneratorManager(_resourceExplorer)));
            turnContext.TurnState.Add(_languagePolicy);
            turnContext.TurnState.Add(_telemetryClient);

            // put this on the TurnState using Set because some adapters (like BotFrameworkAdapter and CloudAdapter) will have already added it
            turnContext.TurnState.Set<BotCallbackHandler>(OnTurnAsync);
        }

        private async Task<Dialog> CreateDialogAsync()
        {
            if (!_resourceExplorer.TryGetResource(_adaptiveDialogId, out var adaptiveDialogResource))
            {
                var msg = $"The ResourceExplorer could not find a resource with id '{_adaptiveDialogId}'";
                _logger.LogError(msg);
                throw new InvalidOperationException(msg);
            }

            var adaptiveDialog = await _resourceExplorer.LoadTypeAsync<AdaptiveDialog>(adaptiveDialogResource, CancellationToken.None).ConfigureAwait(false);

            // if we were passed any Dialogs then add them to the AdaptiveDialog's DialogSet so they can be invoked from any other Dialog
            foreach (var dialog in _dialogs)
            {
                adaptiveDialog.Dialogs.Add(dialog);
            }

            return adaptiveDialog;
        }
    }
}
