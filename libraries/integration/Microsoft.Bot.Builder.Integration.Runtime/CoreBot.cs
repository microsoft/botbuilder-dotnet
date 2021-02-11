// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.Runtime.Settings;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.Integration.Runtime
{
    /// <summary>
    /// Defines the bot runtime standard implementation of <see cref="IBot"/>.
    /// </summary>
    internal class CoreBot : IBot
    {
        private const string DefaultLocale = "en-US";

        private readonly ConversationState _conversationState;
        private readonly DialogManager _dialogManager;
        private readonly UserState _userState;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreBot"/> class.
        /// </summary>
        /// <param name="options">Configured options for the <see cref="CoreBot"/> instance.</param>
        /// <param name="conversationState"><see cref="ConversationState"/> for the bot.</param>
        /// <param name="userState"><see cref="UserState"/> instance for the bot.</param>
        /// <param name="resourceExplorer"><see cref="ResourceExplorer"/> instance to access declarative assets.</param>
        /// <param name="telemetryClient"><see cref="IBotTelemetryClient"/> for the bot.</param>
        /// <param name="botFrameworkClient"><see cref="BotFrameworkClient"/> instance for the bot.</param>
        /// <param name="conversationIdfactory"><see cref="SkillConversationIdFactoryBase"/> instance for the bot.</param>
        public CoreBot(
            IOptions<CoreBotOptions> options,
            ConversationState conversationState,
            UserState userState,
            ResourceExplorer resourceExplorer,
            IBotTelemetryClient telemetryClient,
            BotFrameworkClient botFrameworkClient,
            SkillConversationIdFactoryBase conversationIdfactory)
        {
            _conversationState = conversationState;
            _userState = userState;

            Resource rootDialogResource = resourceExplorer.GetResource(options.Value.RootDialog);
            var rootDialog = resourceExplorer.LoadType<AdaptiveDialog>(rootDialogResource);

            _dialogManager = new DialogManager(rootDialog)
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration()
                .UseLanguagePolicy(new LanguagePolicy(options.Value.DefaultLocale ?? DefaultLocale));

            if (telemetryClient != null)
            {
                _dialogManager.UseTelemetry(telemetryClient);
            }

            _dialogManager.InitialTurnState.Set(botFrameworkClient);
            _dialogManager.InitialTurnState.Set(conversationIdfactory);
            _dialogManager.InitialTurnState.Set(_userState);
            _dialogManager.InitialTurnState.Set(_conversationState);
        }

        /// <summary>
        /// Called by the adapter (for example, a <see cref="BotFrameworkAdapter"/>)
        /// at runtime in order to process an inbound <see cref="Activity"/>.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// This method calls other methods in this class based on the type of the activity to
        /// process, which allows a derived class to provide type-specific logic in a controlled way.
        ///
        /// In a derived class, override this method to add logic that applies to all activity types.
        /// Add logic to apply before the type-specific logic before the call to the base class
        /// <see cref="OnTurnAsync(ITurnContext, CancellationToken)"/> method.
        /// Add logic to apply after the type-specific logic after the call to the base class
        /// <see cref="OnTurnAsync(ITurnContext, CancellationToken)"/> method.
        /// </remarks>
        /// <seealso cref="ActivityHandler.OnMessageActivityAsync(ITurnContext{IMessageActivity}, CancellationToken)"/>
        /// <seealso cref="ActivityHandler.OnConversationUpdateActivityAsync(ITurnContext{IConversationUpdateActivity}, CancellationToken)"/>
        /// <seealso cref="ActivityHandler.OnMessageReactionActivityAsync(ITurnContext{IMessageReactionActivity}, CancellationToken)"/>
        /// <seealso cref="ActivityHandler.OnEventActivityAsync(ITurnContext{IEventActivity}, CancellationToken)"/>
        /// <seealso cref="ActivityHandler.OnUnrecognizedActivityTypeAsync(ITurnContext, CancellationToken)"/>
        /// <seealso cref="Activity.Type"/>
        /// <seealso cref="ActivityTypes"/>
        public virtual async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _dialogManager.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }
    }
}
