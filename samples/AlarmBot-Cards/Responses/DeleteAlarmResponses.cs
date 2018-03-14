// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using AlarmBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Templates;
using Microsoft.Bot.Schema;

namespace AlarmBot.Responses
{
    public static class DeleteAlarmTopicResponses 
    {

        public static IMessageActivity AlarmsCard(IBotContext context, IEnumerable<Alarm> alarms, string title, string message)
        {
            IMessageActivity reply = ((Activity)context.Request).CreateReply();
            var card = new AdaptiveCard();
            card.Body.Add(new TextBlock() { Text = title, Size = TextSize.Large, Wrap = true, Weight = TextWeight.Bolder });
            if (message != null)
                card.Body.Add(new TextBlock() { Text = message, Wrap = true });
            if (alarms.Any())
            {
                FactSet factSet = new FactSet();
                int i = 1;
                foreach (var alarm in alarms)
                    factSet.Facts.Add(new AdaptiveCards.Fact($"{i++}. {alarm.Title}", alarm.Time.Value.ToString("f")));
                card.Body.Add(factSet);

                i = 1;
                reply.SuggestedActions = new SuggestedActions(
                    actions: alarms.Select(alarm =>
                        new CardAction(type: ActionTypes.ImBack,
                            title: $"{i} {alarm.Title}",
                            value: i.ToString(),
                            displayText: i.ToString(),
                            text: i++.ToString())).ToList());
            }
            else
                card.Body.Add(new TextBlock() { Text = "There are no alarms defined", Weight = TextWeight.Lighter });
            reply.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));
            return reply;
        }


        public static void ReplyWithNoAlarms(IBotContext context)
        {
            context.Batch().Reply($"There are no alarms defined.");
        }
        public static void ReplyWithNoAlarmsFound(IBotContext context, string data)
        {
            context.Batch().Reply($"There were no alarms found for {data}.");
        }
        public static void ReplyWithTitlePrompt(IBotContext context, IEnumerable<Alarm> data)
        {
            context.Batch().Reply(AlarmsCard(context, data, "Delete Alarm", "What alarm do you want to delete?"));
        }
        public static void ReplyWithDeletedAlarm(IBotContext context, Alarm data)
        {
            context.Batch().Reply($"I have deleted {data.Title} alarm");
        }

    }
}
