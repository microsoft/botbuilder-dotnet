// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.Topics;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Builder.Core;
using Microsoft.Bot.Builder.Core.State;

namespace AlarmBot
{
    public class AlarmBot : IBot
    {
        public async Task OnTurn(ITurnContext turnContext)
        {
            var context = new AlarmBotContext(turnContext);

            var conversationStateManager = context.ConversationState();
            var topicState = default(AlarmTopicState);

            // Trace top intent
            // await turnContext.SendActivity(context.Activity.CreateTrace("conversationState", value: context.ConversationState));

            var handled = false;

            var conversationUpdateActivity = context.Activity.AsConversationUpdateActivity();

            if (conversationUpdateActivity != null)
            {
                if (conversationUpdateActivity.MembersAdded.Any(m => m.Id == context.Activity.Recipient.Id))
                {
                    topicState = new AlarmTopicState();
                    topicState.ActiveTopic = new DefaultTopic();
                    conversationStateManager.Set(topicState);

                    await topicState.ActiveTopic.StartTopic(context);
                }
            }
            else
            {
                topicState = await conversationStateManager.Get<AlarmTopicState>();

                await turnContext.TraceActivity("AlarmTopicState", value: topicState);

                var handled = await topicState.ActiveTopic.ContinueTopic(context);

                if (handled == false && !(topicState.ActiveTopic is DefaultTopic))
                {
                    topicState.ActiveTopic = new DefaultTopic();

                    conversationStateManager.Set(topicState);

                    await topicState.ActiveTopic.ResumeTopic(context);
                }
            }
        }
    }
}