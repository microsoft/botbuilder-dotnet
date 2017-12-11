using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmBot
{
    public static class TopicViewHelpers
    {
        public static IMessageActivity ReplyWithSuggestions(BotContext context, string title, string message, string[] choices)
        {
            var reply = ReplyWithTitle(context, title, message);

            reply.SuggestedActions = new SuggestedActions(
                actions: choices.Select(choice =>
                    new CardAction(type: ActionTypes.ImBack,
                        title: choice,
                        value: choice.ToLower(),
                        displayText: choice.ToLower(),
                        text: choice.ToLower())).ToList());
            return reply;
        }

        public static IMessageActivity ReplyWithTitle(BotContext context, string title, string message)
        {
            StringBuilder sb = new StringBuilder();
            if (title != null)
                sb.AppendLine($"# {title}\n");

            if (message != null)
                sb.AppendLine(message);

            return context.Request.CreateReply(sb.ToString());
        }


        /// <summary>
        /// Create Yes/No card
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id">id for put on value callback</param>
        /// <param name="title">title for messgebox</param>
        /// <param name="message">message for messagebox</param>
        /// <param name="yesLabel">yes label</param>
        /// <param name="noLabel">no label</param>
        /// <returns></returns>
        public static IMessageActivity CreateMessageBoxCard(BotContext context, string id, string title, string message, string yesLabel, string noLabel)
        {
            IMessageActivity reply = context.Request.CreateReply(message);
            var card = new AdaptiveCard();
            card.Body.Add(new TextBlock() { Text = title, Size = TextSize.Large });
            card.Body.Add(new TextBlock() { Text = message });
            card.Actions.Add(new SubmitAction() { Title = yesLabel, Data = new { Id = id, Action = "Yes" } });
            card.Actions.Add(new SubmitAction() { Title = noLabel, Data = new { Id = id, Action = "No" } });
            reply.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));
            return reply;
        }

    }
}
