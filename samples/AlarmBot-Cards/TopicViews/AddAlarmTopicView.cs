using AdaptiveCards;
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
        public const string CANCELCANCELED = "AddAlarmTopic.CancelCanceled";
        public const string CANCELREPROMPT = "AddAlarmTopic.CancelReprompt";
        public const string TOPICCANCELED = "AddAlarmTopic.TopicCanceled";
        public const string ADDEDALARM = "AddAlarmTopic.AddedAlarm";

        /// <summary>
        /// Standard language alarm description
        /// </summary>
        /// <param name="alarm">the alarm to put on card</param>
        /// <param name="title">title for the card</param>
        /// <param name="message">message for the card </param> 
        /// <param name="submitLabel">label for submit button</param>
        /// <param name="cancelLabel">label for cancel button</param>
        /// <returns>activity ready to submit</returns>
        public static IMessageActivity AlarmCardEditor(BotContext context, Alarm alarm, string title, string message, string submitLabel, string cancelLabel)
        {
            IMessageActivity activity = context.Request.CreateReply();
            if (alarm.Time == null)
                alarm.Time = DateTimeOffset.Now + TimeSpan.FromHours(1);

            string time = alarm.Time.Value.ToString("t");
            string date = alarm.Time.Value.ToString("d");

            var card = new AdaptiveCard();
            card.Body.Add(new TextBlock() { Text = title, Size = TextSize.Large, Wrap = true, Weight = TextWeight.Bolder });
            if (message != null)
                card.Body.Add(new TextBlock() { Text = message, Wrap = true });
            card.Body.Add(new TextInput() { Id = "Title", Value = alarm.Title, Style = TextInputStyle.Text, Placeholder = "Title", IsRequired = true, MaxLength = 50 });
            card.Body.Add(new DateInput() { Id = "Day", Value = date, Placeholder = "Day", IsRequired = false });
            card.Body.Add(new TimeInput() { Id = "Time", Value = time, Placeholder = "Time", IsRequired = true });
            card.Actions.Add(new SubmitAction() { Title = submitLabel, DataJson = "{ Action:'Submit' }" });
            card.Actions.Add(new SubmitAction() { Title = cancelLabel, DataJson = "{ Action:'Cancel'}" });
            activity.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));
            return activity;
        }


        /// <summary>
        /// table of language functions which render output in various languages
        /// </summary>
        public static TemplateDictionary Templates = new TemplateDictionary
        {
            // Default templates
            ["default"] = new TemplateIdMap
                {
                    { STARTTOPIC, (context, data) => AlarmCardEditor(context, data, "Adding Alarm", "Please describe your alarm:", "Submit", "Cancel" ) },
                    { HELP, (context, data) => AlarmCardEditor(context, data, "Adding alarm", $"I am working with you to create an alarm.  Please describe your alarm:.\n\n","Submit", "Cancel") },
                    { CONFUSED, (context, data) => $"I am sorry, I didn't understand: {context.Request.Text}." },
                    { CANCELPROMPT, (context, data) => TopicViewHelpers.CreateMessageBoxCard(context, CANCELPROMPT, "Cancel Alarm?", "Are you sure you want to cancel this alarm?", "Yes", "No") },
                    { CANCELREPROMPT, (context, data) => TopicViewHelpers.CreateMessageBoxCard(context, CANCELPROMPT, "Cancel Alarm?", "Please answer with a Yes or No. Are you sure you want to cancel this alarm?", "Yes", "No") },
                    { TOPICCANCELED, (context, data) => $"OK, I have canceled this alarm." },
                    { ADDEDALARM, (context, data) => $"OK, I have added the alarm {((Alarm)data).Title}." },
                }
        };

    }
}
