using AlarmBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmBot.TopicViews
{
    public class DeleteAlarmTopicView : TemplateRendererMiddleware
    {
        public DeleteAlarmTopicView() : base( new DictionaryRenderer(ReplyTemplates))
        {

        }

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

    }
}
