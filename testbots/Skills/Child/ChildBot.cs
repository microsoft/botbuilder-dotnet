// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples
{
    public class ChildBot : ActivityHandler
    {
        private readonly Dialog _dialog;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private readonly string _connectionName;

        public ChildBot(IConfiguration configuration, MainDialog dialog, ConversationState conversationState, UserState userState)
        {
            _dialog = dialog;
            _conversationState = conversationState;
            _userState = userState;
            _connectionName = configuration.GetSection("ConnectionName")?.Value;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            await _conversationState.LoadAsync(turnContext, true, cancellationToken);
            await _userState.LoadAsync(turnContext, true, cancellationToken);
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.ChannelId != "emulator")
            {
                if (turnContext.Activity.Text == "skill login")
                {
                    await _conversationState.LoadAsync(turnContext, true, cancellationToken);
                    await _userState.LoadAsync(turnContext, true, cancellationToken);
                    await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                    return;
                }
                else if (turnContext.Activity.Text == "skill logout")
                {
                    await turnContext.TurnState.Get<UserTokenClient>().SignOutUserAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, cancellationToken).ConfigureAwait(false);
                    await turnContext.SendActivityAsync(MessageFactory.Text("logout from child bot successful"), cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("child: activity (1)"), cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text("child: activity (2)"), cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text("child: activity (3)"), cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text($"child: {turnContext.Activity.Text}"), cancellationToken);
            }
        }
    }
}
