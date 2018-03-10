// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveCards;
using AlarmBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace AlarmBot.Responses
{
    public static class AddAlarmTopicResponses
    {
        public const string CANCELPROMPT = "AddAlarmTopic.Cancelation";
        public const string CANCELREPROMPT = "AddAlarmTopic.CancelReprompt";

        /// <summary>
        /// Standard language alarm description
        /// </summary>
        /// <param name="alarm">the alarm to put on card</param>
        /// <param name="title">title for the card</param>
        /// <param name="message">message for the card </param> 
        /// <param name="submitLabel">label for submit button</param>
        /// <param name="cancelLabel">label for cancel button</param>
        /// <returns>activity ready to submit</returns>
        public static IMessageActivity AlarmCardEditor(IBotContext context, Alarm alarm, string title, string message, string submitLabel, string cancelLabel)
        {
            IMessageActivity activity = ((Activity)context.Request).CreateReply();
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


        public static void ReplyWithStartTopic(IBotContext context, dynamic data)
        {
            context.Batch().Reply(AlarmCardEditor(context, data, "Adding Alarm", "Please describe your alarm:", "Submit", "Cancel"));
        }
        public static void ReplyWithHelp(IBotContext context, dynamic data)
        {
            context.Batch().Reply(AlarmCardEditor(context, data, "Adding alarm", $"I am working with you to create an alarm.  Please describe your alarm:.\n\n", "Submit", "Cancel"));
        }
        public static void ReplyWithConfused(IBotContext context, dynamic data)
        {
            context.Batch().Reply($"I am sorry, I didn't understand: {((Activity)context.Request).Text}.");
        }
        public static void ReplyWithCancelPrompt(IBotContext context, dynamic data)
        {
            context.Batch().Reply(TopicResponseHelpers.CreateMessageBoxCard(context, CANCELPROMPT, "Cancel Alarm?", "Are you sure you want to cancel this alarm?", "Yes", "No"));
        }
        public static void ReplyWithCancelReprompt(IBotContext context, dynamic data)
        {
            context.Batch().Reply(TopicResponseHelpers.CreateMessageBoxCard(context, CANCELPROMPT, "Cancel Alarm?", "Please answer with a Yes or No. Are you sure you want to cancel this alarm?", "Yes", "No"));
        }
        public static void ReplyWithTopicCanceled(IBotContext context, dynamic data)
        {
            context.Batch().Reply($"OK, I have canceled creating this alarm.");
        }
        public static void ReplyWithAddedAlarm(IBotContext context, dynamic data)
        {
            context.Batch().Reply($"OK, I have added the alarm {((Alarm)data).Title}.");
        }
    }
}
