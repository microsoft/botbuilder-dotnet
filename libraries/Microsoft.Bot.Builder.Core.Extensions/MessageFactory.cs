// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Core.Extensions
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
        /// <param name="inputHint">
        /// (Optional)Input hint to the channel on what the bot is expecting. 
        /// Possible values include: 'acceptingInput',
        /// 'ignoringInput', 'expectingInput'
        /// </param>
        public static Activity Text(string text, string ssml = null, string inputHint = null)
        {
            IMessageActivity ma = Activity.CreateMessageActivity();
            SetTextAndSpeak(ma, text, ssml, inputHint);
            return (Activity)ma;
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
        /// <param name="inputHint">
        /// (Optional)Input hint to the channel on what the bot is expecting. 
        /// Possible values include: 'acceptingInput',
        /// 'ignoringInput', 'expectingInput'
        /// </param>
        public static IMessageActivity SuggestedActions(IList<string> actions, string text = null, string ssml = null, string inputHint = null)
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

            return SuggestedActions(cardActions, text, ssml, inputHint);
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
        /// <param name="inputHint">
        /// (Optional)Input hint to the channel on what the bot is expecting. 
        /// Possible values include: 'acceptingInput',
        /// 'ignoringInput', 'expectingInput'
        /// </param>
        public static IMessageActivity SuggestedActions(IList<CardAction> cardActions, string text = null, string ssml = null, string inputHint = null)
        {
            if (cardActions == null)
                throw new ArgumentNullException(nameof(cardActions));

            IMessageActivity ma = Activity.CreateMessageActivity();
            SetTextAndSpeak(ma, text, ssml, inputHint);

            ma.SuggestedActions = new SuggestedActions {Actions = cardActions};

            return ma;
        }

        /// <summary>
        /// Returns a single message activity containing an attachment.
        /// </summary>
        /// <param name="attachment">Adaptive card to include in the message</param>
        /// <param name="text">(Optional) text of the message. </param>
        /// <param name="ssml">(Optional) SSML to include with the message.</param>
        /// <param name="inputHint">(Optional)Input hint to the channel on what the bot is expecting.</param>
        /// <returns>Message activity containing an attachment</returns>
        public static IMessageActivity Attachment(Attachment attachment, string text = null, string ssml = null, string inputHint = null)
        {
            if (attachment == null)
                throw new ArgumentNullException(nameof(attachment));

            return Attachment(new List<Attachment> { attachment }, text, ssml, inputHint);
        }

        /// <summary>
        /// Returns a message that will display a set of attachments in list form.
        /// </summary>
        /// <param name="attachment">List of attachments to include in the message.</param>
        /// <param name="text">(Optional) text of the message. </param>
        /// <param name="ssml">(Optional) SSML to include with the message.</param>
        /// <param name="inputHint">(Optional)Input hint to the channel on what the bot is expecting.</param>
        /// <returns>Message activity containing the attachment list.</returns>
        public static IMessageActivity Attachment(IList<Attachment> attachments, string text = null, string ssml = null, string inputHint = null)
        {
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));

            return AttachmentActivity(AttachmentLayoutTypes.List, attachments, text, ssml, inputHint);
        }

        /// <summary>
        /// Returns a message that will display a set of attachments using a carousel layout.
        /// </summary>
        /// <param name="attachments">List of attachments to include in the message.</param>
        /// <param name="text">(Optional) text of the message.</param>
        /// <param name="ssml">(Optional) SSML to include with the message.</param>
        /// <param name="inputHint">(Optional)Input hint to the channel on what the bot is expecting.</param>
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
        public static IMessageActivity Carousel(IList<Attachment> attachments, string text = null, string ssml = null, string inputHint = null)
        {
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));

            return AttachmentActivity(AttachmentLayoutTypes.Carousel, attachments, text, ssml, inputHint);
        }

        /// <summary>
        /// Returns a message that will display a single image or video to a user.
        /// </summary>
        /// <param name="url">Url of the image/video to send.</param>
        /// <param name="contentType">The MIME type of the image/video.</param>
        /// <param name="name">(Optional) Name of the image/video file.</param>
        /// <param name="text">(Optional) text of the message.</param>
        /// <param name="ssml">(Optional) SSML to include with the message.</param>
        /// <param name="inputHint">(Optional)Input hint to the channel on what the bot is expecting.</param>
        /// <returns>
        /// Returns a message that will display a single image or video to a user.
        /// </returns>
        /// <example>
        /// <code>
        ///     IMessageActivity message = 
        ///         MessageFactory.ContentUrl("https://{domainName}/cat.jpg", MediaTypeNames.Image.Jpeg, "Cat Picture");
        /// </code>
        /// </example>
        public static IMessageActivity ContentUrl(string url, string contentType, string name = null, string text = null, string ssml = null, string inputHint = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentNullException(nameof(contentType));

            Attachment a = new Attachment
            {
                ContentType = contentType,
                ContentUrl = url,
                Name = !string.IsNullOrWhiteSpace(name) ? name : string.Empty
            };

            return AttachmentActivity(AttachmentLayoutTypes.List, new List<Attachment> { a }, text, ssml, inputHint);
        }

        private static IMessageActivity AttachmentActivity(string attachmentLayout, IList<Attachment> attachments, string text = null, string ssml = null, string inputHint = null)
        {
            IMessageActivity ma = Activity.CreateMessageActivity();
            ma.AttachmentLayout = attachmentLayout;
            ma.Attachments = attachments;
            SetTextAndSpeak(ma, text, ssml, inputHint);
            return ma;
        }

        private static void SetTextAndSpeak(IMessageActivity ma, string text = null, string ssml = null, string inputHint = null)
        {
            // Note: we must put NULL in the fields, as the clients will happily render 
            // an empty string, which is not the behavior people expect to see. 
            ma.Text = !string.IsNullOrWhiteSpace(text) ? text : null;
            ma.Speak = !string.IsNullOrWhiteSpace(ssml) ? ssml : null;

            ma.InputHint = inputHint;
        }
    }
}
