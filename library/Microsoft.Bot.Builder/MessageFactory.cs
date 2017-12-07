using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    public static class MessageFactory
    {
        public static IMessageActivity Text(string text, string ssml = null)
        {
            IMessageActivity ma = Activity.CreateMessageActivity();
            SetTextAndSpeak(ma, text, ssml);
            return ma;
        }

        public static IMessageActivity SuggestedActions(IList<string> actions, string text = null, string ssml = null)
        {
            if (actions == null)
                throw new ArgumentNullException(nameof(actions));

            IList<CardAction> cardActions = new List<CardAction>();
            foreach (string s in actions)
            {
                CardAction ca = new CardAction
                {
                    Type = ActionTypes.ImBack,
                    Value = s,
                    Title = s
                };

                cardActions.Add(ca);
            }

            return SuggestedActions(cardActions, text, ssml);
        }
        public static IMessageActivity SuggestedActions(IList<CardAction> cardActions, string text = null, string ssml = null)
        {
            if (cardActions == null)
                throw new ArgumentNullException(nameof(cardActions));

            IMessageActivity ma = Activity.CreateMessageActivity();
            SetTextAndSpeak(ma, text, ssml);

            ma.SuggestedActions = new Connector.SuggestedActions();
            ma.SuggestedActions.Actions = cardActions;

            return ma;
        }

        public static IMessageActivity Attachment(Attachment attachment, string text = null, string ssml = null)
        {
            if (attachment == null)
                throw new ArgumentNullException(nameof(attachment));

            return Attachment(new List<Attachment> { attachment }, text, ssml);
        }

        public static IMessageActivity Attachment(IList<Attachment> attachments, string text = null, string ssml = null)
        {
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));

            return AttachmentActivity(AttachmentLayoutTypes.List, attachments, text, ssml);
        }

        public static IMessageActivity Carousel(IList<Attachment> attachments, string text = null, string ssml = null)
        {
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));

            return AttachmentActivity(AttachmentLayoutTypes.Carousel, attachments, text, ssml);
        }

        public static IMessageActivity ContentUrl(string url, string contentType, string name = null, string text = null, string ssml = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentNullException(nameof(contentType));

            Attachment a = new Connector.Attachment
            {
                ContentType = contentType,
                ContentUrl = url,
                Name = !string.IsNullOrWhiteSpace(name) ? name : string.Empty
            };

            return AttachmentActivity(AttachmentLayoutTypes.List, new List<Attachment> { a }, text, ssml);
        }

        private static IMessageActivity AttachmentActivity(string attachmentLayout, IList<Attachment> attachments, string text = null, string ssml = null)
        {
            IMessageActivity ma = Activity.CreateMessageActivity();
            ma.AttachmentLayout = attachmentLayout;
            ma.Attachments = attachments;
            SetTextAndSpeak(ma, text, ssml);
            return ma;
        }

        private static void SetTextAndSpeak(IMessageActivity ma, string text = null, string ssml = null)
        {
            ma.Text = !string.IsNullOrWhiteSpace(text) ? text : string.Empty;
            ma.Speak = !string.IsNullOrWhiteSpace(ssml) ? ssml : string.Empty;
        }
    }
}
