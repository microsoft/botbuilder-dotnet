// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
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
    public class CoreBot : ActivityHandler
    {
        private const string DefaultLocale = "en-US";

        private readonly ConversationState _conversationState;
        private readonly DialogManager _dialogManager;
        private readonly UserState _userState;

        public CoreBot(IServiceProvider services, IOptions<CoreBotOptions> options)
        {
            this._conversationState = services.GetRequiredService<ConversationState>();
            this._userState = services.GetRequiredService<UserState>();

            this._dialogManager = CreateDialogManager(services, options);
        }

        public override async Task OnTurnAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken = default(CancellationToken))
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
