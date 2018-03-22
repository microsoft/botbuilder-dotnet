// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using AlarmBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AlarmBot.Responses
{
    public static class DeleteAlarmTopicResponses 
    {
        public static IMessageActivity AlarmsCard(ITurnContext context, IEnumerable<Alarm> alarms, string title, string message)
        {
            IMessageActivity reply = context.Activity.CreateReply();
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

        public static async Task ReplyWithNoAlarms(ITurnContext context)
        {
            await context.SendActivity($"There are no alarms defined.");
        }
        public static async Task ReplyWithNoAlarmsFound(ITurnContext context, string data)
        {
            await context.SendActivity($"There were no alarms found for {data}.");
        }
        public static async Task ReplyWithTitlePrompt(ITurnContext context, IEnumerable<Alarm> data)
        {
            await context.SendActivity(AlarmsCard(context, data, "Delete Alarm", "What alarm do you want to delete?"));
        }
        public static async Task ReplyWithDeletedAlarm(ITurnContext context, Alarm data)
        {
            await context.SendActivity($"I have deleted {data.Title} alarm");
        }

    }
}
