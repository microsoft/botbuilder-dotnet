// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// An <see cref="IBot"/> implementation that hosts an <see cref="AdaptiveDialog"/>.
    /// </summary>
    public class AdaptiveDialogBot : IBot
    {
        private const string DefaultLocaleKey = "defaultLocale";
        private const string RootDialogKey = "defaultRootDialog";

        private const string DefaultLocale = "en-US";
        private const string DefaultLg = "main.lg";

        private static ConcurrentDictionary<ResourceExplorer, LanguageGeneratorManager> _languageGeneratorManagers = new ConcurrentDictionary<ResourceExplorer, LanguageGeneratorManager>();

        private readonly string _defaultLocale;
        private readonly string _rootDialogId;

        private readonly ILogger _logger;
        private readonly ResourceExplorer _resourceExplorer;
        private readonly IStorage _storage;
        private readonly BotFrameworkClient _botFrameworkClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveDialogBot"/> class.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <param name="logger">An <see cref="ILogger"/> instance.</param>
        /// <param name="storage">The <see cref="IStorage"/> implementation to use for this <see cref="Dialog"/>.</param>
        /// <param name="resourceExplorer">The Bot Builder <see cref="ResourceExplorer"/> to load the <see cref="Dialog"/> from.</param>
        /// <param name="botFrameworkClient">A <see cref="BotFrameworkClient"/> for making calls to Bot Builder Skills.</param>
        public AdaptiveDialogBot(
            IConfiguration configuration,
            ILogger<AdaptiveDialogBot> logger,
            ResourceExplorer resourceExplorer,
            IStorage storage = null,
            BotFrameworkClient botFrameworkClient = null)
        {
            _defaultLocale = configuration.GetSection(DefaultLocaleKey).Value ?? DefaultLocale;
            _rootDialogId = configuration.GetSection(RootDialogKey).Value ?? "Main.dialog";

            _logger = logger;
            _resourceExplorer = resourceExplorer;
            _storage = storage ?? new MemoryStorage();
            _botFrameworkClient = botFrameworkClient;
        }

        async Task IBot.OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"IBot.OnTurnContext for dialog '{_rootDialogId}'");

            // Set up the TurnState the Dialog is expecting
            SetUpTurnState(turnContext);

            // Load the Dialog from the ResourceExplorer
            var rootDialog = await CreateDialogAsync(cancellationToken).ConfigureAwait(false);

            // Run the Dialog
            await rootDialog.RunAsync(turnContext, turnContext.TurnState.Get<ConversationState>().CreateProperty<DialogState>("DialogState"), cancellationToken).ConfigureAwait(false);

            // Save any updates that have been made
            await turnContext.TurnState.Get<ConversationState>().SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
            await turnContext.TurnState.Get<UserState>().SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }

        private void SetUpTurnState(ITurnContext turnContext)
        {
            turnContext.TurnState.Add(_botFrameworkClient);
            turnContext.TurnState.Add<SkillConversationIdFactoryBase>(new SkillConversationIdFactory(_storage));
            turnContext.TurnState.Add(new ConversationState(_storage));
            turnContext.TurnState.Add(new UserState(_storage));
            turnContext.TurnState.Add(_resourceExplorer);
            turnContext.TurnState.Add<LanguageGenerator>(_resourceExplorer.TryGetResource(DefaultLg, out var resource) ? new ResourceMultiLanguageGenerator(DefaultLg) : new TemplateEngineLanguageGenerator());
            turnContext.TurnState.Add(_languageGeneratorManagers.GetOrAdd(_resourceExplorer, _ => new LanguageGeneratorManager(_resourceExplorer)));
            turnContext.TurnState.Add(new LanguagePolicy(_defaultLocale));
        }

        private async Task<Dialog> CreateDialogAsync(CancellationToken cancellationToken)
        {
            if (!_resourceExplorer.TryGetResource(_rootDialogId, out var rootDialogResource))
            {
                var msg = $"The ResourceExplorer could not find a resourced with id '{_rootDialogId}'";
                _logger.LogError(msg);
                throw new Exception(msg);
            }

            return await _resourceExplorer.LoadTypeAsync<AdaptiveDialog>(rootDialogResource, cancellationToken).ConfigureAwait(false);
        }
    }
}
