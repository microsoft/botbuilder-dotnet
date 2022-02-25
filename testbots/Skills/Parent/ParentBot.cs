// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{
    public class ParentBot : ActivityHandler
    {
        private BotFrameworkAuthentication _authentication;
        private readonly Dialog _dialog;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private readonly string _toBotId;
        private readonly string _fromBotId;
        private readonly string _connectionName;

        // regex to check if code supplied is a 6 digit numerical code (hence, a magic code).
        private readonly Regex _magicCodeRegex = new Regex(@"(\d{6})");

        public ParentBot(BotFrameworkAuthentication authentication, IConfiguration configuration, MainDialog dialog, ConversationState conversationState, UserState userState)
        {
            _authentication = authentication;
            _dialog = dialog;
            _conversationState = conversationState;
            _userState = userState;
            _fromBotId = configuration.GetSection("MicrosoftAppId")?.Value;
            _toBotId = configuration.GetSection("SkillMicrosoftAppId")?.Value;
            _connectionName = configuration.GetSection("ConnectionName")?.Value;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // for signin, just use an oauth prompt to get the exchangeable token
            // also ensure that the channelId is not emulator
            if (turnContext.Activity.ChannelId != "emulator")
            {
                if (_magicCodeRegex.IsMatch(turnContext.Activity.Text) || turnContext.Activity.Text == "login")
                {
                    // start an oauth prompt
                    await _conversationState.LoadAsync(turnContext, true, cancellationToken);
                    await _userState.LoadAsync(turnContext, true, cancellationToken);
                    await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                }
                else if (turnContext.Activity.Text == "logout")
                {
                    var adapter = turnContext.Adapter as IExtendedUserTokenProvider;
                    await adapter.SignOutUserAsync(turnContext, _connectionName, turnContext.Activity.From.Id, cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text("logout from parent bot successful"), cancellationToken);
                }
                else if (turnContext.Activity.Text == "skill login" || turnContext.Activity.Text == "skill logout")
                {
                    // incoming activity needs to be cloned for buffered replies
                    var cloneActivity = MessageFactory.Text(turnContext.Activity.Text);
                    cloneActivity.ApplyConversationReference(turnContext.Activity.GetConversationReference(), true);
                    cloneActivity.DeliveryMode = DeliveryModes.ExpectReplies;

                    using (var client = _authentication.CreateBotFrameworkClient())
                    {
                        var response1 = await client.PostActivityAsync<ExpectedReplies>(
                            _fromBotId,
                            _toBotId,
                            new Uri("http://localhost:2303/api/messages"),
                            new Uri("http://tempuri.org/whatever"),
                            turnContext.Activity.Conversation.Id,
                            cloneActivity,
                            cancellationToken);

                        if (response1.Status == (int)HttpStatusCode.OK && response1.Body?.Activities.Count() > 0)
                        {
                            var activities = response1.Body.Activities.ToArray();
                            if (!(await InterceptOAuthCards(activities, turnContext, cancellationToken)))
                            {
                                await turnContext.SendActivitiesAsync(activities, cancellationToken);
                            }
                        }
                    }
                }

                return;
            }

            await turnContext.SendActivityAsync(MessageFactory.Text("parent: before child"), cancellationToken);

            var activity = MessageFactory.Text("parent to child");
            activity.ApplyConversationReference(turnContext.Activity.GetConversationReference(), true);
            activity.DeliveryMode = DeliveryModes.ExpectReplies;

            using (var client = _authentication.CreateBotFrameworkClient())
            {
                var response = await client.PostActivityAsync<ExpectedReplies>(
                    _fromBotId,
                    _toBotId,
                    new Uri("http://localhost:2303/api/messages"),
                    new Uri("http://tempuri.org/whatever"),
                    Guid.NewGuid().ToString(),
                    activity,
                    cancellationToken);

                if (response.Status == (int)HttpStatusCode.OK)
                {
                    await turnContext.SendActivitiesAsync(response.Body?.Activities.ToArray(), cancellationToken);
                }
            }

            await turnContext.SendActivityAsync(MessageFactory.Text("parent: after child"), cancellationToken);
        }

        private async Task<bool> InterceptOAuthCards(Activity[] activities, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (activities.Length == 0)
            {
                return false;
            }

            var activity = activities[0];
            if (activity.Attachments != null)
            {
                foreach (var attachment in activity.Attachments.Where(a => a?.ContentType == OAuthCard.ContentType))
                {
                    var oauthCard = ((JObject)attachment.Content).ToObject<OAuthCard>();
                    if (oauthCard.TokenExchangeResource != null)
                    {
                        // AAD token exchange
                        var tokenExchangeProvider = turnContext.Adapter as IExtendedUserTokenProvider;
                        var result = await tokenExchangeProvider.ExchangeTokenAsync(
                            turnContext,
                            _connectionName,
                            turnContext.Activity.From.Id,
                            new TokenExchangeRequest(oauthCard.TokenExchangeResource.Uri));

                        if (!string.IsNullOrWhiteSpace(result.Token))
                        {
                            // Send an invoke back to the skill
                            return await SendTokenExchangeInvokeToSkill(turnContext, activity, oauthCard.TokenExchangeResource.Id, oauthCard.ConnectionName, result.Token, cancellationToken);
                        }
                    }
                }
            }

            return false;
        }

        private async Task<bool> SendTokenExchangeInvokeToSkill(ITurnContext turnContext, Activity incomingActivity, string id, string connectionName, string token, CancellationToken cancellationToken)
        {
            var activity = incomingActivity.CreateReply() as Activity;
            activity.Type = ActivityTypes.Invoke;
            activity.Name = "signin/tokenExchange";
            activity.Value = new TokenExchangeInvokeRequest()
            {
                Id = id,
                Token = token,
                ConnectionName = connectionName
            };

            // route the activity to the skill
            using (var client = _authentication.CreateBotFrameworkClient())
            {
                var response = await client.PostActivityAsync(
                    _fromBotId,
                    _toBotId,
                    new Uri("http://localhost:2303/api/messages"),
                    new Uri("http://tempuri.org/whatever"),
                    incomingActivity.Conversation.Id,
                    activity,
                    cancellationToken);

                // Check response status: true if success, false if failure
                var success = IsSucessStatusCode(response.Status);
                if (success)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Skill token exchange successful"), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Skill token exchange failed"), cancellationToken);
                }

                return success;
            }
        }

        private bool IsSucessStatusCode(int statusCode)
        {
            return statusCode >= 200 && statusCode <= 299;
        }
    }
}
