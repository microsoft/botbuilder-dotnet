// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// An <see cref="IBot"/> implementation that hosts an <see cref="AdaptiveDialog"/>.
    /// </summary>
    public class AdaptiveDialogBot : IBot
    {
        private const string DefaultLocale = "en-US";
        private const string DefaultLg = "main.lg";
        private const string DefaultDialogName = "main.dialog";

        private static readonly ConcurrentDictionary<ResourceExplorer, LanguageGeneratorManager> _languageGeneratorManagers = new ConcurrentDictionary<ResourceExplorer, LanguageGeneratorManager>();

        private readonly string _defaultLocale;
        private readonly string _rootDialogId;

        private readonly ILogger _logger;
        private readonly ResourceExplorer _resourceExplorer;
        private readonly BotFrameworkAuthentication _botFrameworkAuthentication;

        private readonly SkillConversationIdFactoryBase _skillConversationIdFactoryBase;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveDialogBot"/> class.
        /// </summary>
        /// <param name="resourceExplorer">The Bot Builder <see cref="ResourceExplorer"/> to load the <see cref="Dialog"/> from.</param>
        /// <param name="rootDialogId">The id of the dialog to load from the <see cref="ResourceExplorer"/>.</param>
        /// <param name="defaultLocale">The default locale for this bot.</param>
        /// <param name="logger">An <see cref="ILogger"/> instance.</param>
        /// <param name="storage">The <see cref="IStorage"/> implementation to use for this <see cref="Dialog"/>.</param>
        /// <param name="botFrameworkAuthentication">A <see cref="BotFrameworkAuthentication"/> used to obtain a client for making calls to Bot Builder Skills.</param>
        public AdaptiveDialogBot(
            ResourceExplorer resourceExplorer,
            string rootDialogId,
            string defaultLocale,
            ILogger<AdaptiveDialogBot> logger = null,
            IStorage storage = null,
            BotFrameworkAuthentication botFrameworkAuthentication = null)
            : this(
                  resourceExplorer,
                  rootDialogId ?? DefaultDialogName,
                  defaultLocale ?? DefaultLocale,
                  logger ?? NullLogger<AdaptiveDialogBot>.Instance,
                  botFrameworkAuthentication ?? BotFrameworkAuthenticationFactory.Create(),
                  new SkillConversationIdFactory(storage ?? new MemoryStorage()),
                  new ConversationState(storage ?? new MemoryStorage()),
                  new UserState(storage ?? new MemoryStorage()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveDialogBot"/> class.
        /// </summary>
        /// <param name="resourceExplorer">The Bot Builder <see cref="ResourceExplorer"/> to load the <see cref="Dialog"/> from.</param>
        /// <param name="rootDialogId">The id of the dialog to load from the <see cref="ResourceExplorer"/>.</param>
        /// <param name="defaultLocale">The default locale for this bot.</param>
        /// <param name="logger">An <see cref="ILogger"/> instance.</param>
        /// <param name="botFrameworkAuthentication">A <see cref="BotFrameworkAuthentication"/> used to obtain a client for making calls to Bot Builder Skills.</param>
        /// <param name="skillConversationIdFactoryBase">A <see cref="SkillConversationIdFactoryBase"/> implementation.</param>
        /// <param name="conversationState">A <see cref="ConversationState"/> implementation.</param>
        /// <param name="userState">A <see cref="UserState"/> implementation.</param>
        public AdaptiveDialogBot(
            ResourceExplorer resourceExplorer,
            string rootDialogId,
            string defaultLocale,
            ILogger<AdaptiveDialogBot> logger,
            BotFrameworkAuthentication botFrameworkAuthentication,
            SkillConversationIdFactoryBase skillConversationIdFactoryBase,
            ConversationState conversationState,
            UserState userState)
        {
            _resourceExplorer = resourceExplorer;
            _rootDialogId = rootDialogId ?? "Main.dialog";
            _defaultLocale = defaultLocale ?? DefaultLocale;
            _logger = logger ?? NullLogger<AdaptiveDialogBot>.Instance;
            _botFrameworkAuthentication = botFrameworkAuthentication ?? BotFrameworkAuthenticationFactory.Create();

            _skillConversationIdFactoryBase = skillConversationIdFactoryBase;
            _conversationState = conversationState;
            _userState = userState;
        }

        async Task IBot.OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"IBot.OnTurnContext for dialog '{_rootDialogId}'");

            using (var botFrameworkClient = _botFrameworkAuthentication.CreateBotFrameworkClient())
            {
                // Set up the TurnState the Dialog is expecting
                SetUpTurnState(turnContext, botFrameworkClient);

                // Load the Dialog from the ResourceExplorer
                var rootDialog = await CreateDialogAsync(cancellationToken).ConfigureAwait(false);

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
            turnContext.TurnState.Add(_resourceExplorer.TryGetResource(DefaultLg, out var resource) ? (LanguageGenerator)new ResourceMultiLanguageGenerator(DefaultLg) : new TemplateEngineLanguageGenerator());
            turnContext.TurnState.Add(_languageGeneratorManagers.GetOrAdd(_resourceExplorer, _ => new LanguageGeneratorManager(_resourceExplorer)));
            turnContext.TurnState.Add(new LanguagePolicy(_defaultLocale));
        }

        private async Task<Dialog> CreateDialogAsync(CancellationToken cancellationToken)
        {
            if (!_resourceExplorer.TryGetResource(_rootDialogId, out var rootDialogResource))
            {
                var msg = $"The ResourceExplorer could not find a resourced with id '{_rootDialogId}'";
                _logger.LogError(msg);
                throw new InvalidOperationException(msg);
            }

            return await _resourceExplorer.LoadTypeAsync<AdaptiveDialog>(rootDialogResource, cancellationToken).ConfigureAwait(false);
        }
    }
}
