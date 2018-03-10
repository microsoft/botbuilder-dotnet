// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using AlarmBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Templates;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Linq;

namespace AlarmBot.Responses
{
    public static class ShowAlarmsTopicResponses 
    {
        public static IMessageActivity AlarmsCard(IBotContext context, IEnumerable<Alarm> alarms, string title, string message)
        {
            IMessageActivity activity = ((Activity)context.Request).CreateReply(message);

            var card = new AdaptiveCard();
            card.Body.Add(new TextBlock() { Text = title, Size = TextSize.Large, Wrap = true, Weight = TextWeight.Bolder });

            if (message != null)
                card.Body.Add(new TextBlock() { Text = message, Wrap = true });

            if (alarms.Any())
            {
                FactSet factSet = new FactSet();

                foreach (var alarm in alarms)
                    factSet.Facts.Add(new AdaptiveCards.Fact(alarm.Title, alarm.Time.Value.ToString("f")));

                card.Body.Add(factSet);
            }
            else
                card.Body.Add(new TextBlock() { Text = "There are no alarms defined", Weight = TextWeight.Lighter });

            activity.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));
            return activity;
        }

        public static void ReplyWithShowAlarms(IBotContext context, dynamic data)
        {
            context.Batch().Reply(AlarmsCard(context, data, "Alarms", null));
        }
    }
}
