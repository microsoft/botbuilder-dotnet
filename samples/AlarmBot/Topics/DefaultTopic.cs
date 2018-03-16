// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.Responses;
using Microsoft.Bot.Schema;

namespace AlarmBot.Topics
{
    /// <summary>
    /// Class around root default topic 
    /// </summary>
    public class DefaultTopic : ITopic
    {
        public string Name { get; set; } = "Default";

        // track in this topic if we have greeted the user already
        public bool Greeted { get; set; } = false;

        /// <summary>
        /// Called when the default topic is started
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> StartTopic(AlarmBotContext context)
        {
            switch (context.Request.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    {
                        // greet when added to conversation
                        var activity = context.Request.AsConversationUpdateActivity();
                        if (activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
                        {
                            await DefaultResponses.ReplyWithGreeting(context);
                            await DefaultResponses.ReplyWithHelp(context);
                            this.Greeted = true;
                        }
                    }
                    break;

                case ActivityTypes.Message:
                    // greet on first message if we haven't already 
                    if (!Greeted)
                    {
                        await DefaultResponses.ReplyWithGreeting(context);
                        this.Greeted = true;
                    }
                    return await this.ContinueTopic(context);
            }

            return true;
        }

        /// <summary>
        /// Continue the topic, method which is routed to while this topic is active
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> ContinueTopic(AlarmBotContext context)
        {
            switch (context.Request.Type)
            {
                case ActivityTypes.Message:
                    switch (context.RecognizedIntents.TopIntent?.Name)
                    {
                        case "addAlarm":
                            // switch to addAlarm topic
                            context.ConversationState.ActiveTopic = new AddAlarmTopic();
                            return await context.ConversationState.ActiveTopic.StartTopic(context);                            
                        case "showAlarms":
                            // switch to show alarms topic
                            context.ConversationState.ActiveTopic = new ShowAlarmsTopic();
                            return await context.ConversationState.ActiveTopic.StartTopic(context);                            
                        case "deleteAlarm":
                            // switch to delete alarm topic
                            context.ConversationState.ActiveTopic = new DeleteAlarmTopic();
                            return await context.ConversationState.ActiveTopic.StartTopic(context);                            
                        case "help":
                            // show help
                            await DefaultResponses.ReplyWithHelp(context);
                            return true;

                        default:
                            // show our confusion
                            await DefaultResponses.ReplyWithConfused(context);
                            return true;
                    }                    
                default:
                    break;
            }

            return true;
        }

        /// <summary>
        /// Method which is called when this topic is resumed after an interruption
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> ResumeTopic(AlarmBotContext context)
        {
            // just prompt the user to ask what they want to do
            await DefaultResponses.ReplyWithResumeTopic(context);
            return true;
        }
    }
}
