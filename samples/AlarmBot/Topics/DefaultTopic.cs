using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Templates;
using AlarmBot.Models;

namespace AlarmBot.Topics
{
    public class DefaultTopic : ITopic
    {
        public const string TopicName = "DefaultTopic";

        // template ids
        public const string GREETING = "DefaultTopic.StartTopic";
        public const string RESUMETOPIC = "DefaultTopic.ResumeTopic";
        public const string HELP = "DefaultTopic.Help";
        public const string CONFUSED = "DefaultTopic.Confusion";

        // template functions for rendeing responses in different a languages
        public static TemplateDictionary ReplyTemplates = new TemplateDictionary
        {
            ["default"] = new TemplateIdMap
                {
                    { DefaultTopic.GREETING, (context, data) => $"Hello, I'm the alarmbot." },
                    { DefaultTopic.HELP, (context, data) => $"I can add an alarm, show alarms or delete an alarm. " },
                    { DefaultTopic.RESUMETOPIC, (context, data) => $"What can I do for you?" },
                    { DefaultTopic.CONFUSED, (context, data) => $"I am sorry, I didn't understand that." },
                },
            ["en"] = new TemplateIdMap { },
            ["fr"] = new TemplateIdMap { }
        };

        public DefaultTopic() { }

        // track in this topic if we have greeted the user already
        public bool Greeted { get; set; } = false;

        /// <summary>
        /// Called when the default topic is started
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> StartTopic(BotContext context)
        {
            switch (context.Request.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    {
                        // greet when added to conversation
                        var activity = context.Request.AsConversationUpdateActivity();
                        if (activity.MembersAdded.Where(m => m.Id == activity.Recipient.Id).Any())
                        {
                            context.ReplyWith(GREETING);
                            context.ReplyWith(HELP);
                            this.Greeted = true;
                        }
                    }
                    break;

                case ActivityTypes.Message:
                    // greet on first message if we haven't already 
                    if (!Greeted)
                    {
                        context.ReplyWith(GREETING);
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
        public Task<bool> ContinueTopic(BotContext context)
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
                            context.ReplyWith(HELP);
                            return Task.FromResult(true);

                        default:
                            // show our confusion
                            context.ReplyWith(CONFUSED);
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
        public Task<bool> ResumeTopic(BotContext context)
        {
            // just prompt the user to ask what they want to do
            context.ReplyWith(RESUMETOPIC);
            return Task.FromResult(true);
        }

    }
}
