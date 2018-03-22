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
        public string Name { get; set; } = "Default";

        // track in this topic if we have greeted the user already
        public bool Greeted { get; set; } = false;

        /// <summary>
        /// Called when the default topic is started
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> StartTopic(ITurnContext context)
        {
            switch (context.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    {
                        // greet when added to conversation
                        var activity = context.Activity.AsConversationUpdateActivity();
                        if (activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
                        {
                            await DefaultTopicResponses.ReplyWithGreeting(context);
                            await DefaultTopicResponses.ReplyWithHelp(context);
                            this.Greeted = true;
                        }
                    }
                    break;

                case ActivityTypes.Message:
                    // greet on first message if we haven't already 
                    if (!Greeted)
                    {
                        await DefaultTopicResponses.ReplyWithGreeting(context);
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
        public async Task<bool> ContinueTopic(ITurnContext context)
        {
            var conversation = ConversationState<ConversationData>.Get(context);         
            var recognizedIntents = context.Services.Get<IRecognizedIntents>();
            switch (context.Activity.Type)
            {
                case ActivityTypes.Message:                    
                    switch (recognizedIntents.TopIntent?.Name)
                    {
                        case "addAlarm":
                            // switch to addAlarm topic
                            conversation.ActiveTopic = new AddAlarmTopic();
                            return await conversation.ActiveTopic.StartTopic(context);

                        case "showAlarms":
                            // switch to show alarms topic
                            conversation.ActiveTopic = new ShowAlarmsTopic();
                            return await conversation.ActiveTopic.StartTopic(context);

                        case "deleteAlarm":
                            // switch to delete alarm topic
                            conversation.ActiveTopic = new DeleteAlarmTopic();
                            return await conversation.ActiveTopic.StartTopic(context);

                        case "help":
                            // show help
                            await DefaultTopicResponses.ReplyWithHelp(context);
                            return true;

                        default:
                            // show our confusion
                            await DefaultTopicResponses.ReplyWithConfused(context);
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
        public async Task<bool> ResumeTopic(ITurnContext context)
        {
            // just prompt the user to ask what they want to do
            await DefaultTopicResponses.ReplyWithResumeTopic(context);
            return true;
        }
    }
}
