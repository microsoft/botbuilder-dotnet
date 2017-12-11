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
    public class DeleteAlarmTopicView : TemplateRendererMiddleware
    {
        public DeleteAlarmTopicView() : base(new DictionaryRenderer(ReplyTemplates))
        {

        }

        public static IMessageActivity AlarmsCard(BotContext context, IEnumerable<Alarm> alarms, string title, string message)
        {
            IMessageActivity reply = context.Request.CreateReply();
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
                    { TITLEPROMPT, (context, data) => AlarmsCard(context, data, "Delete Alarm", "What alarm do you want to delete?") },
                    { DELETEDALARM, (context, data) => $"I have deleted {((Alarm)data).Title} alarm" },
                }
        };

    }
}
