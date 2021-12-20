// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.OAuth;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class Bot : TeamsActivityHandler
    {
        private const string UserLoggingInPropertyName = "UserLoggingIn";
        private const string ConfigurationConnectionName = "ConnectionName";
        private readonly BotState _conversationState;
        private readonly string _connectionName;
        private readonly BotState _userState;
        private readonly IStatePropertyAccessor<bool> _userLoggingInProperty;
        private readonly OAuthCardProvider _oauthCardProvider;

        public Bot(IConfiguration configuration, ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
            _userState = userState;
            _connectionName = configuration[ConfigurationConnectionName].ToString();
            _userLoggingInProperty = conversationState.CreateProperty<bool>(UserLoggingInPropertyName);
            var oauthSettings = new OAuthSettings { ConnectionName = _connectionName, Title = "Login", Text = "Please Sign In to proceed..." };
            _oauthCardProvider = new OAuthCardProvider(oauthSettings);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            return CompleteLoginWithResponse(turnContext, cancellationToken);
        }

        protected override Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            return CompleteLoginWithResponse(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text;
            if (text.Equals("logout", StringComparison.OrdinalIgnoreCase))
            {
                var userTokenClient = turnContext.TurnState.Get<UserTokenClient>() ?? throw new InvalidOperationException("The UserTokenClient is not supported by the current adapter.");
                await userTokenClient.SignOutUserAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, cancellationToken).ConfigureAwait(false);

                await turnContext.SendActivityAsync(MessageFactory.Text($"You have been logged out."), cancellationToken).ConfigureAwait(false);
            }
            else if (text.Equals("login", StringComparison.OrdinalIgnoreCase))
            {
                await _oauthCardProvider.SendOAuthCardAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
                await _userLoggingInProperty.SetAsync(turnContext, true).ConfigureAwait(false);
            }
            else
            {
                if (await _userLoggingInProperty.GetAsync(turnContext, () => false).ConfigureAwait(false))
                {
                    await CompleteLoginWithResponse(turnContext, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"You said {text}"), cancellationToken).ConfigureAwait(false);
                }
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to AuthenticationBot. Type anything to get logged in. Type 'logout' to sign-out."), cancellationToken);
                }
            }
        }

        private async Task CompleteLoginWithResponse(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var token = await _oauthCardProvider.RecognizeTokenAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(token?.Token))
            {
                await _userLoggingInProperty.SetAsync(turnContext, false).ConfigureAwait(false);
                await turnContext.SendActivityAsync(MessageFactory.Text($"You are signed in! Here is your token:"), cancellationToken).ConfigureAwait(false);
                await turnContext.SendActivityAsync(MessageFactory.Text(token.Token), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"You are not signed in."), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}



//protected async override Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
//{
//    if (turnContext.Activity.Name == SignInConstants.TokenExchangeOperationName) 
//    {
//        await LoginUserWithResponse(turnContext, cancellationToken).ConfigureAwait(false);
//    }

//    await base.OnSignInInvokeAsync(turnContext, cancellationToken).ConfigureAwait(false);
//}
//protected override Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
//{
//    return base.OnInvokeActivityAsync(turnContext, cancellationToken);
//}

//protected override Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
//{
//    return base.OnEventActivityAsync(turnContext, cancellationToken);
//}
