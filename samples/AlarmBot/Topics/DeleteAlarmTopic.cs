using AlarmBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Templates;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlarmBot.Topics
{
    public class DeleteAlarmTopic : ITopic
    {
        public const string TopicName = "DeleteAlarmTopic";

        // template ids
        public const string NOALARMS = "DeleteAlarmTopic.NoAlarms";
        public const string NOALARMSFOUND = "DeleteAlarmTopic.NoAlarmsFound";
        public const string TITLEPROMPT = "DeleteAlarmTopic.TitlePrompt";
        public const string DELETEDALARM = "DeleteAlarmTopic.DeletedAlarm";

        // per language template functions for creating replies
        public static TemplateDictionary ReplyTemplates = new TemplateDictionary
        {
            ["default"] = new TemplateIdMap
                {
                    { NOALARMS, (context, data) => $"There are no alarms defined." },
                    { NOALARMSFOUND, (context, data) => $"There were no alarms found for {(string)data}." },
                    { TITLEPROMPT, (context, data) => $"# Delete Alarm\n\nWhat alarm do you want to delete?" },
                    { DELETEDALARM, (context, data) => $"I have deleted {((Alarm)data).Title} alarm" },
                }
        };

        /// <summary>
        /// The alarm title we are searching for
        /// </summary>
        public string AlarmTitle { get; set; }

        /// <summary>
        /// Called when the topic is started
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> StartTopic(BotContext context)
        {
            this.AlarmTitle = context.TopIntent?.Entities.Where(entity => entity.GroupName == "AlarmTitle")
                                .Select(entity => entity.ValueAs<string>()).FirstOrDefault();

            return FindAlarm(context);
        }

        /// <summary>
        /// Called for every turn while active
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> ContinueTopic(BotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                this.AlarmTitle = context.Request.Text.Trim();
                return await this.FindAlarm(context);
            }
            return true;
        }

        /// <summary>
        /// Called when resuming from an interruption
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> ResumeTopic(BotContext context)
        {
            return this.FindAlarm(context);
        }

        public async Task<bool> FindAlarm(BotContext context)
        {
            var alarms = (List<Alarm>)context.State.User[UserProperties.ALARMS];
            if (alarms == null)
            {
                alarms = new List<Alarm>();
                context.State.User[UserProperties.ALARMS] = alarms;
            }

            // Ensure there are alarms to delete
            if (alarms.Count == 0)
            {
                context.ReplyWith(NOALARMS);
                return false;
            }

            // Validate title
            if (!String.IsNullOrWhiteSpace(this.AlarmTitle))
            {
                var parts = this.AlarmTitle.Split(' ');
                var choices = alarms.FindAll(alarm => parts.Any(part => alarm.Title.Contains(part)));

                if (choices.Count == 0)
                {
                    context.ReplyWith(NOALARMSFOUND, this.AlarmTitle);
                    return false;
                }
                else if (choices.Count == 1)
                {
                    // Delete selected alarm and end topic
                    alarms.Remove(choices.First());
                    context.ReplyWith(DELETEDALARM, choices.First());
                    return false; // cancel topic
                }
            }

            // Prompt for title
            await ShowAlarmsTopic.ShowAlarms(context);
            context.ReplyWith(TITLEPROMPT, alarms);
            return true;
        }
    }
}
