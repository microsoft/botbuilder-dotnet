// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples
{
    public class ParentBot : ActivityHandler
    {
        private BotFrameworkHttpClient _client;
        private readonly Dialog _dialog;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private readonly string _toBotId;
        private readonly string _fromBotId;
        private readonly string _connectionName;
        private readonly SkillsHelper _skillsHelper;

        // regex to check if code supplied is a 6 digit numerical code (hence, a magic code).
        private readonly Regex _magicCodeRegex = new Regex(@"(\d{6})");

        public ParentBot(SkillHttpClient client, IConfiguration configuration, MainDialog dialog, ConversationState conversationState, UserState userState, SkillsHelper skillsHelper)
        {
            _client = client;
            _dialog = dialog;
            _conversationState = conversationState;
            _userState = userState;
            _fromBotId = configuration.GetSection("MicrosoftAppId")?.Value;
            _toBotId = configuration.GetSection("SkillMicrosoftAppId")?.Value;
            _connectionName = configuration.GetSection("ConnectionName")?.Value;
            _skillsHelper = skillsHelper;
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
                    var response1 = await _skillsHelper.PostActivityAsync(cloneActivity, cancellationToken) as InvokeResponse<ExpectedReplies>;

                    if (response1 != null && response1.Status == (int)HttpStatusCode.OK && response1.Body?.Activities != null)
                    {
                        var activities = response1.Body.Activities.ToArray();
                        if (!(await _skillsHelper.InterceptOAuthCards(activities, cancellationToken)))
                        {
                            await turnContext.SendActivitiesAsync(activities, cancellationToken);
                        }
                    }
                }

                return;
            }

            await turnContext.SendActivityAsync(MessageFactory.Text("parent: before child"), cancellationToken);

            var activity = MessageFactory.Text("parent to child");
            activity.ApplyConversationReference(turnContext.Activity.GetConversationReference(), true);
            activity.DeliveryMode = DeliveryModes.ExpectReplies;

            var response = await _client.PostActivityAsync<ExpectedReplies>(
                _fromBotId,
                _toBotId,
                new Uri("http://localhost:2303/api/messages"),
                new Uri("http://tempuri.org/whatever"),
                Guid.NewGuid().ToString(),
                activity,
                cancellationToken);

            if (response.Status == (int)HttpStatusCode.OK)
            {
                await turnContext.SendActivitiesAsync(response.Body?.Activities?.ToArray(), cancellationToken);
            }

            await turnContext.SendActivityAsync(MessageFactory.Text("parent: after child"), cancellationToken);
        }
    }
}
