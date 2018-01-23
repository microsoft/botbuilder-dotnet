// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlarmBot.Models;
using AlarmBot.Topics;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Templates;
using Microsoft.Bot.Connector;

namespace AlarmBot.TopicViews
{
    public class DeleteAlarmTopicView : TemplateRendererMiddleware
    {
        public DeleteAlarmTopicView() : base(new DictionaryRenderer(ReplyTemplates))
        {

        }

        // template ids
        public const string NOALARMS = "DeleteAlarmTopic.NoAlarms";
        public const string NOALARMSFOUND = "DeleteAlarmTopic.NoAlarmsFound";
        public const string TITLEPROMPT = "DeleteAlarmTopic.TitlePrompt";
        public const string DELETEDALARM = "DeleteAlarmTopic.DeletedAlarm";

        public static IMessageActivity GetDeleteActivity(IBotContext context, IEnumerable<Alarm> alarms, string title, string message)
        {
            StringBuilder sb = new StringBuilder();
            int i = 1;
            if (alarms.Any())
            {
                foreach (var alarm in alarms)
                    sb.AppendLine($"{i++}. {alarm.Title} {alarm.Time.Value.ToString("f")}");
            }
            else
                sb.AppendLine("There are no alarms defined");
            i = 1;
            return TopicViewHelpers.ReplyWithSuggestions(context,
                title,
                $"{message}\n\n{sb.ToString()}",
                alarms.Select(alarm => $"{i++} {alarm.Title}").ToArray());
        }

        // per language template functions for creating replies
        public static TemplateDictionary ReplyTemplates = new TemplateDictionary
        {
            ["default"] = new TemplateIdMap
                {
                    { NOALARMS, (context, data) => $"There are no alarms defined." },
                    { NOALARMSFOUND, (context, data) => $"There were no alarms found for {(string)data}." },
                    { TITLEPROMPT, (context, data) => GetDeleteActivity(context, ShowAlarmsTopic.GetAlarms(context), "Delete Alarms", "What alarm do you want to delete?") },
                    { DELETEDALARM, (context, data) => $"I have deleted {((Alarm)data).Title} alarm" },
                }
        };

    }
}
