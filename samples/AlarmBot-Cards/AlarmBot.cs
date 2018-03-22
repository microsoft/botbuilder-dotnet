// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.Topics;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.State;

namespace AlarmBot
{
    public class AlarmBot : IBot
    {
        public async Task OnTurn(ITurnContext context)
        {
            var conversationStateManager = turnContext.ConversationState();
            var topicState = default(AlarmTopicState);

            var conversationUpdateActivity = turnContext.Activity.AsConversationUpdateActivity();

            if (conversationUpdateActivity != null)
            {
                if (conversationUpdateActivity.MembersAdded.Any(m => m.Id == turnContext.Activity.Recipient.Id))
                {
                    topicState = new AlarmTopicState();
                    topicState.ActiveTopic = new DefaultTopic();
                    conversationStateManager.Set(topicState);

                    await topicState.ActiveTopic.StartTopic(turnContext);
                }
            }
            else
            {
                topicState = await conversationStateManager.Get<AlarmTopicState>();

                var handled = await topicState.ActiveTopic.ContinueTopic(turnContext);

                if (handled == false && !(topicState.ActiveTopic is DefaultTopic))
                {
                    topicState.ActiveTopic = new DefaultTopic();

                    conversationStateManager.Set(topicState);

                    await topicState.ActiveTopic.ResumeTopic(turnContext);
                }
            }
        }
    }
}