using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A set of utility functions to assist with the formatting of the various 
    /// message types a bot can return.
    /// </summary/>
    /// <example>
    /// <code>    
    /// var message = MessageFactory.Text("Hello World");    
    /// context.reply(message); // send message    
    /// </code>
    /// </example>           
    public static class MessageFactory
    {
        /// <summary>
        /// Returns a simple text message.
        /// </summary/>
        /// <example>
        /// <code>    
        /// var message = MessageFactory.Text("Hello World");    
        /// context.reply(message);
        /// </code>
        /// </example>
        /// <param name="text">
        /// Text to include in the message
        /// </param>
        /// <param name="ssml">
        /// (Optional) SSML to include in the message.
        /// </param>
        public static IMessageActivity Text(string text, string ssml = null)
        {
            IMessageActivity ma = Activity.CreateMessageActivity();
            SetTextAndSpeak(ma, text, ssml);
            return ma;
        }

        /// <summary>
        /// Returns a message that includes a set of suggested actions and optional text.
        /// </summary/>
        /// <example>
        /// <code>    
        /// var message = MessageFactory.Text("Hello World");    
        /// context.reply(message);
        /// </code>
        /// </example>
        /// <param name="actions">
        /// List of actions to include. String Actions are converted to <see cref="Microsoft.Bot.Connector.ActionTypes.ImBack"/>.
        /// </param>        
        /// <param name="text">
        /// (Optional) text of the message. 
        /// </param>
        /// <param name="ssml">
        /// (Optional) SSML to include in the message.
        /// </param>
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
        /// <summary>
        /// Returns a message that includes a set of suggested actions and optional text.
        /// </summary/>
        /// <example>
        /// <code>    
        /// var message = MessageFactory.Text("Hello World");    
        /// context.reply(message);
        /// </code>
        /// </example>
        /// <param name="actions">
        /// List of <see cref="Microsoft.Bot.Connector.ActionTypes"/> to include.
        /// </param>        
        /// <param name="text">
        /// (Optional) text of the message. 
        /// </param>
        /// <param name="ssml">
        /// (Optional) SSML to include in the message.
        /// </param>
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

        /// <summary>
        /// Returns a single message activity containing an attachment.
        /// </summary>
        /// <param name="attachment">Adaptive card to include in the message</param>
        /// <param name="text">(Optional) text of the message. </param>
        /// <param name="ssml">(Optional) SSML to include with the message.</param>
        /// <returns>Message activity containing an attachment</returns>
        public static IMessageActivity Attachment(Attachment attachment, string text = null, string ssml = null)
        {
            if (attachment == null)
                throw new ArgumentNullException(nameof(attachment));

            return Attachment(new List<Attachment> { attachment }, text, ssml);
        }

        /// <summary>
        /// Returns a message that will display a set of attachments in list form.
        /// </summary>
        /// <param name="attachment">List of attachments to include in the message.</param>
        /// <param name="text">(Optional) text of the message. </param>
        /// <param name="ssml">(Optional) SSML to include with the message.</param>
        /// <returns>Message activity containing the attachment list.</returns>
        public static IMessageActivity Attachment(IList<Attachment> attachments, string text = null, string ssml = null)
        {
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));

            return AttachmentActivity(AttachmentLayoutTypes.List, attachments, text, ssml);
        }

        /// <summary>
        /// Returns a message that will display a set of attachments using a carousel layout.
        /// </summary>
        /// <param name="attachments">List of attachments to include in the message.</param>
        /// <param name="text">(Optional) text of the message.</param>
        /// <param name="ssml">(Optional) SSML to include with the message.</param>
        /// <returns>
        /// Returns a message that will display a set of attachments using a carousel layout.
        /// </returns>
        /// <example>
        /// <code>
        ///     IList<Attachment> multipleAttachments = 
        ///             new List<Attachment> { attachment1, attachment2 };
        ///             
        ///     IMessageActivity message = 
        ///             MessageFactory.Carousel(multipleAttachments, text, ssml);
        /// </code>
        /// </example>
        public static IMessageActivity Carousel(IList<Attachment> attachments, string text = null, string ssml = null)
        {
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));

            return AttachmentActivity(AttachmentLayoutTypes.Carousel, attachments, text, ssml);
        }

        /// <summary>
        /// Returns a message that will display a single image or video to a user.
        /// </summary>
        /// <param name="url">Url of the image/video to send.</param>
        /// <param name="contentType">The MIME type of the image/video.</param>
        /// <param name="name">(Optional) Name of the image/video file.</param>
        /// <param name="text">(Optional) text of the message.</param>
        /// <param name="ssml">(Optional) SSML to include with the message.</param>
        /// <returns>
        /// Returns a message that will display a single image or video to a user.
        /// </returns>
        /// <example>
        /// <code>
        ///     IMessageActivity message = 
        ///         MessageFactory.ContentUrl("https://{domainName}/cat.jpg", MediaTypeNames.Image.Jpeg, "Cat Picture");
        /// </code>
        /// </example>
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
