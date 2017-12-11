using AlarmBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Templates;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmBot.TopicViews
{
    public class AddAlarmTopicView : TemplateRendererMiddleware
    {
        public AddAlarmTopicView() : base(new DictionaryRenderer(Templates))
        {
        }

        // template ids
        public const string STARTTOPIC = "AddAlarmTopic.StartTopic";
        public const string RESUMETOPIC = "AddAlarmTopic.ResumeTopic";
        public const string HELP = "AddAlarmTopic.Help";
        public const string CONFUSED = "AddAlarmTopic.Confusion";
        public const string CANCELPROMPT = "AddAlarmTopic.Cancelation";
        public const string TOPICCANCELED = "AddAlarmTopic.TopicCanceled";
        public const string CANCELCANCELED = "AddAlarmTopic.CancelCanceled";
        public const string CANCELREPROMPT = "AddAlarmTopic.CancelReprompt";
        public const string TITLEPROMPT = "AddAlarmTopic.TitlePrompt";
        public const string TITLEVALIDATIONPROMPT = "AddAlarmTopic.TitleValidationPrompt";
        public const string TIMEPROMPT = "AddAlarmTopic.TimePrompt";
        public const string TIMEVALIDATIONPROMPT = "AddAlarmTopic.TimeValidationPrompt";
        public const string ADDEDALARM = "AddAlarmTopic.AddedAlarm";
        public const string ADDCONFIRMATION = "AddAlarmTopic.AddConfirmation";
        public const string TIMEPROMPTFUTURE = "AddAlarmTopic.TimePromptFuture";

        /// <summary>
        /// Standard language alarm description
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string AlarmDescription(BotContext context, Alarm alarm)
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(alarm.Title))
                sb.AppendLine($"* Title: {alarm.Title}");
            else
                sb.AppendLine($"* Title: -");

            if (alarm.Time != null)
            {
                if (alarm.Time.Value.DayOfYear == DateTimeOffset.Now.DayOfYear)
                    sb.AppendLine($"* Time: {alarm.Time.Value.ToString("t")}");
                else
                    sb.AppendLine($"* Time: {alarm.Time.Value.ToString("f")}");
            }
            else
                sb.AppendLine($"* Time: -");
            return sb.ToString();
        }

        public static string[] YesNo = { "Yes", "No" };

        /// <summary>
        /// table of language functions which render output in various languages
        /// </summary>
        public static TemplateDictionary Templates = new TemplateDictionary
        {
            // Default templates
            ["default"] = new TemplateIdMap
                {
                    { STARTTOPIC, (context, data) => $"Ok, let's add an alarm." },
                    { HELP, (context, data) => $"I am working with you to create an alarm.  To do that I need to know the title and time.\n\n{AlarmDescription(context,data)}"},
                    { CONFUSED, (context, data) => $"I am sorry, I didn't understand: {context.Request.Text}." },
                    { CANCELPROMPT, (context, data) => TopicViewHelpers.ReplyWithSuggestions(context, "Cancel Alarm?", $"Did you want to cancel the alarm?\n\n{AlarmDescription(context,data)}", YesNo) },
                    { CANCELREPROMPT, (context, data) => TopicViewHelpers.ReplyWithSuggestions(context, $"Cancel alarm?", $"Please answer the question with a \"yes\" or \"no\" reply. Did you want to cancel the alarm?\n\n{AlarmDescription(context,data)}", YesNo) },
                    { TOPICCANCELED, (context, data) => $"OK, I have canceled this alarm." },
                    { TIMEPROMPT, (context, data) => TopicViewHelpers.ReplyWithTitle(context, $"Adding alarm", $"{AlarmDescription(context,data)}\n\nWhat time would you like to set the alarm for?") },
                    { TIMEPROMPTFUTURE, (context, data) => TopicViewHelpers.ReplyWithTitle(context, $"Adding alarm",$"{AlarmDescription(context,data)}\n\nYou need to specify a time in the future. What time would you like to set the alarm?") },
                    { TITLEPROMPT, (context, data)=> TopicViewHelpers.ReplyWithTitle(context, $"Adding alarm",$"{AlarmDescription(context,data)}\n\nWhat would you like to call your alarm ?") },
                    { ADDCONFIRMATION, (context, data)=> TopicViewHelpers.ReplyWithSuggestions(context, $"Adding Alarm",$"{AlarmDescription(context,data)}\n\nDo you want to save this alarm?", YesNo) },
                    { ADDEDALARM, (context, data)=> TopicViewHelpers.ReplyWithTitle(context, $"Alarm Added",$"{AlarmDescription(context,data)}.") }
                }
        };

    }
}
