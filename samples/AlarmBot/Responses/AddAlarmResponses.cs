// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using AlarmBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace AlarmBot.Responses
{
    /// <summary>
    /// I organized all of my output responses as functions so it is easy to reuse and evolve the responses over time without having to rewrite my business logic
    /// </summary>
    public static class AddAlarmResponses
    {
        public static void ReplyWithStartTopic(IBotContext context)
        {
            context.Batch().Reply($"Ok, let's add an alarm.");
        }

        public static void ReplyWithHelp(IBotContext context, Alarm alarm = null)
        {
            context.Batch().Reply($"I am working with you to create an alarm.  To do that I need to know the title and time.\n\n{AlarmDescription(context, alarm)}");
        }

        public static void ReplyWithConfused(IBotContext context)
        {
            context.Batch().Reply($"I am sorry, I didn't understand: {context.Request.AsMessageActivity().Text}.");
        }

        public static void ReplyWithCancelPrompt(IBotContext context, Alarm alarm)
        {
            context.Batch().Reply(ResponseHelpers.ReplyWithSuggestions(context, "Cancel Alarm?", $"Did you want to cancel the alarm?\n\n{AlarmDescription(context, alarm)}", YesNo));
        }

        public static void ReplyWithCancelReprompt(IBotContext context, Alarm alarm)
        {
            context.Batch().Reply(ResponseHelpers.ReplyWithSuggestions(context, $"Cancel alarm?", $"Please answer the question with a \"yes\" or \"no\" reply. Did you want to cancel the alarm?\n\n{AlarmDescription(context, alarm)}", YesNo));
        }

        public static void ReplyWithTopicCanceled(IBotContext context) => context.Batch().Reply($"OK, I have canceled this alarm.");

        public static void ReplyWithTimePrompt(IBotContext context, Alarm alarm)
        {
            context.Batch().Reply(ResponseHelpers.ReplyWithTitle(context, $"Adding alarm", $"{AlarmDescription(context, alarm)}\n\nWhat time would you like to set the alarm for?"));
        }

        public static void ReplyWithTimePromptFuture(IBotContext context, Alarm alarm)
        {
            context.Batch().Reply(ResponseHelpers.ReplyWithTitle(context, $"Adding alarm", $"{AlarmDescription(context, alarm)}\n\nYou need to specify a time in the future. What time would you like to set the alarm?"));
        }

        public static void ReplyWithTitlePrompt(IBotContext context, Alarm alarm)
        {
            context.Batch().Reply(ResponseHelpers.ReplyWithTitle(context, $"Adding alarm", $"{AlarmDescription(context, alarm)}\n\nWhat would you like to call your alarm ?"));
        }

        public static void ReplyWithTitleValidationPrompt(IBotContext context, Alarm alarm)
        {
            context.Batch().Reply(ResponseHelpers.ReplyWithTitle(context, $"Adding alarm", $"Your title needs to be between 1 and 100 characterslong\n\n{AlarmDescription(context, alarm)}\n\nWhat would you like to call your alarm ?"));
        }

        public static void ReplyWithAddConfirmation(IBotContext context, Alarm alarm)
        {
            context.Batch().Reply(ResponseHelpers.ReplyWithSuggestions(context, $"Adding Alarm", $"{AlarmDescription(context, alarm)}\n\nDo you want to save this alarm?", YesNo));
        }

        public static void ReplyWithAddedAlarm(IBotContext context, Alarm alarm)
        {
            context.Batch().Reply(ResponseHelpers.ReplyWithTitle(context, $"Alarm Added", $"{AlarmDescription(context, alarm)}."));
        }

        /// <summary>
        /// Standard language alarm description
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string AlarmDescription(IBotContext context, Alarm alarm)
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


    }
}
