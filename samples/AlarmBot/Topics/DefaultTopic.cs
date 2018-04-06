// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using AlarmBot.Models;
using AlarmBot.Responses;
using Microsoft.Bot.Builder;
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
            switch (context.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    {
                        // greet when added to conversation
                        var activity = context.Activity.AsConversationUpdateActivity();
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
            switch (context.Activity.Type)
            {
                case ActivityTypes.Message:
                    var conversationStateManager = context.ConversationState();
                    var conversationData = await conversationStateManager.GetOrCreate<AlarmTopicState>();
                    var result = true;

                    switch (context.RecognizedIntents.TopIntent?.Name)
                    {
                        case "addAlarm":
                            // switch to addAlarm topic
                            conversationData.ActiveTopic = new AddAlarmTopic();
                            result = await conversationData.ActiveTopic.StartTopic(context);

                            break;

                        case "showAlarms":
                            // switch to show alarms topic
                            conversationData.ActiveTopic = new ShowAlarmsTopic();
                            result = await conversationData.ActiveTopic.StartTopic(context);

                            break;

                        case "deleteAlarm":
                            // switch to delete alarm topic
                            conversationData.ActiveTopic = new DeleteAlarmTopic();
                            result = await conversationData.ActiveTopic.StartTopic(context);

                            break;

                        case "help":
                            // show help
                            await DefaultResponses.ReplyWithHelp(context);

                            break;

                        default:
                            // show our confusion
                            await DefaultResponses.ReplyWithConfused(context);

                            break;
                    }

                    conversationStateManager.Set(conversationData);

                    return result;
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
