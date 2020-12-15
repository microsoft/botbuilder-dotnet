// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Runtime.Settings;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.Runtime
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
        /// <param name="services">Services registered with the application.</param>
        /// <param name="options">Configured options for the <see cref="CoreBot"/> instance.</param>
        public CoreBot(IServiceProvider services, IOptions<CoreBotOptions> options)
        {
            this._conversationState = services.GetRequiredService<ConversationState>();
            this._userState = services.GetRequiredService<UserState>();

            this._dialogManager = CreateDialogManager(services, options);
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
            var rootDialog = (AdaptiveDialog)this._dialogManager.RootDialog;
            if (turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity claimIdentity &&
                SkillValidation.IsSkillClaim(claimIdentity.Claims))
            {
                rootDialog.AutoEndDialog = true;
            }

            await this._dialogManager.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            await this._conversationState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
            await this._userState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }

        private static DialogManager CreateDialogManager(IServiceProvider services, IOptions<CoreBotOptions> options)
        {
            var resourceExplorer = services.GetRequiredService<ResourceExplorer>();
            var telemetryClient = services.GetService<IBotTelemetryClient>();

            Resource rootDialogResource = resourceExplorer.GetResource(options.Value.RootDialog);
            var rootDialog = resourceExplorer.LoadType<AdaptiveDialog>(rootDialogResource);

            var dialogManager = new DialogManager(rootDialog)
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration()
                .UseLanguagePolicy(new LanguagePolicy(options.Value.DefaultLocale ?? DefaultLocale));

            if (telemetryClient != null)
            {
                dialogManager.UseTelemetry(telemetryClient);
            }

            dialogManager.InitialTurnState.Set(services.GetRequiredService<BotFrameworkClient>());
            dialogManager.InitialTurnState.Set(services.GetRequiredService<SkillConversationIdFactoryBase>());

            return dialogManager;
        }
    }
}
