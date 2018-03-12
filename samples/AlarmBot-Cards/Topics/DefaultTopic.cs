// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.Responses;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace AlarmBot.Topics
{
    /// <summary>
    /// Class around root default topic
    /// </summary>
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
                            DefaultTopicResponses.ReplyWithGreeting(context);
                            DefaultTopicResponses.ReplyWithHelp(context);
                            this.Greeted = true;
                        }
                    }
                    break;

                case ActivityTypes.Message:
                    // greet on first message if we haven't already 
                    if (!Greeted)
                    {
                        DefaultTopicResponses.ReplyWithGreeting(context);
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
            var conversation = ConversationState<ConversationData>.Get(context);
         // var conversation = context.GetConversationState<ConversationData>();
            var recognizedIntents = context.Get<IRecognizedIntents>();
            switch (context.Request.Type)
            {
                case ActivityTypes.Message:                    
                    switch (recognizedIntents.TopIntent?.Name)
                    {
                        case "addAlarm":
                            // switch to addAlarm topic
                            conversation.ActiveTopic = new AddAlarmTopic();
                            return conversation.ActiveTopic.StartTopic(context);

                        case "showAlarms":
                            // switch to show alarms topic
                            conversation.ActiveTopic = new ShowAlarmsTopic();
                            return conversation.ActiveTopic.StartTopic(context);

                        case "deleteAlarm":
                            // switch to delete alarm topic
                            conversation.ActiveTopic = new DeleteAlarmTopic();
                            return conversation.ActiveTopic.StartTopic(context);

                        case "help":
                            // show help
                            DefaultTopicResponses.ReplyWithHelp(context);
                            return Task.FromResult(true);

                        default:
                            // show our confusion
                            DefaultTopicResponses.ReplyWithConfused(context);
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
            DefaultTopicResponses.ReplyWithResumeTopic(context);
            return Task.FromResult(true);
        }
    }
}
