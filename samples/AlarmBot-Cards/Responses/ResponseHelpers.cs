// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AlarmBot
{
    public static class TopicResponseHelpers
    {
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
        public static IMessageActivity CreateMessageBoxCard(ITurnContext context, string id, string title, string message, string yesLabel, string noLabel)
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
