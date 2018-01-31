// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.TopicViews;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AlarmBot.Topics
{
    public class DefaultTopic : ITopic
    {
        public DefaultTopic() { }

        public string Name { get; set; } = "Default";

        // track in this topic if we have greeted the user already
        public bool Greeted { get; set; } = false;

        /// <summary>
        /// Called when the default topic is started
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> StartTopic(IBotContext context)
        {
            switch (context.Request.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    {
                        // greet when added to conversation
                        var activity = context.Request.AsConversationUpdateActivity();
                        if (activity.MembersAdded.Where(m => m.Id == activity.Recipient.Id).Any())
                        {
                            context.ReplyWith(DefaultTopicView.GREETING);
                            context.ReplyWith(DefaultTopicView.HELP);
                            this.Greeted = true;
                        }
                    }
                    break;

                case ActivityTypes.Message:
                    // greet on first message if we haven't already 
                    if (!Greeted)
                    {
                        context.ReplyWith(DefaultTopicView.GREETING);
                        this.Greeted = true;
                    }
                    return this.ContinueTopic(context);
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// Continue the topic, method which is routed to while this topic is active
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> ContinueTopic(IBotContext context)
        {
            var activeTopic = (ITopic)context.State.Conversation[ConversationProperties.ACTIVETOPIC];

            switch (context.Request.Type)
            {
                case ActivityTypes.Message:
                    switch (context.TopIntent?.Name)
                    {
                        case "addAlarm":
                            // switch to addAlarm topic
                            activeTopic = new AddAlarmTopic();
                            context.State.Conversation[ConversationProperties.ACTIVETOPIC] = activeTopic;
                            return activeTopic.StartTopic(context);

                        case "showAlarms":
                            // switch to show alarms topic
                            activeTopic = new ShowAlarmsTopic();
                            context.State.Conversation[ConversationProperties.ACTIVETOPIC] = activeTopic;
                            return activeTopic.StartTopic(context);

                        case "deleteAlarm":
                            // switch to delete alarm topic
                            activeTopic = new DeleteAlarmTopic();
                            context.State.Conversation[ConversationProperties.ACTIVETOPIC] = activeTopic;
                            return activeTopic.StartTopic(context);

                        case "help":
                            // show help
                            context.ReplyWith(DefaultTopicView.HELP);
                            return Task.FromResult(true);

                        default:
                            // show our confusion
                            context.ReplyWith(DefaultTopicView.CONFUSED);
                            return Task.FromResult(true);
                    }

                default:
                    break;
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// Method which is called when this topic is resumed after an interruption
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> ResumeTopic(IBotContext context)
        {
            // just prompt the user to ask what they want to do
            context.ReplyWith(DefaultTopicView.RESUMETOPIC);
            return Task.FromResult(true);
        }
    }
}
