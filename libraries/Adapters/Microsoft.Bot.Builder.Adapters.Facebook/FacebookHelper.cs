// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    public static class FacebookHelper
    {
        /// <summary>
        /// Converts an Activity object to a Facebook messenger outbound message ready for the API.
        /// </summary>
        /// <param name="activity">The activity to be converted to Facebook message.</param>
        /// <returns>The resulting message.</returns>
        public static FacebookMessage ActivityToFacebook(Activity activity)
        {
            if (activity == null)
            {
                return null;
            }

            var facebookMessage = new FacebookMessage(activity.Conversation.Id, new Message(), "RESPONSE");

            facebookMessage.Message.Text = activity.Text;

            // map these fields to their appropriate place
            if (activity.ChannelData != null)
            {
                facebookMessage = activity.GetChannelData<FacebookMessage>();

                // make sure the quick reply has a type
                if (activity.GetChannelData<FacebookMessage>().Message.QuickReplies != null)
                {
                    facebookMessage.Message.QuickReplies = activity.GetChannelData<FacebookMessage>().Message.QuickReplies; // TODO: Add the content_type depending of what shape quick_replies has
                }
            }

            if (activity.Attachments != null && activity.Attachments.Count > 0)
            {
                if (activity.Attachments.Count > 1)
                {
                    throw new Exception("Facebook message can only contain one attachment");
                }

                var url = new Uri(activity.Attachments[0].ContentUrl);
                var attach = new FacebookAttachment
                {
                    Type = activity.Attachments[0].ContentType,
                    Payload = new MessagePayload { Url = url },
                };

                facebookMessage.Message.Attachment = attach;
            }

            return facebookMessage;
        }

        /// <summary>
        /// Handles each individual message inside a webhook payload (webhook may deliver more than one message at a time).
        /// </summary>
        /// <param name="message">The message to be processed.</param>
        /// <returns>An Activity with the result.</returns>
        public static Activity ProcessSingleMessage(FacebookMessage message)
        {
            if (message == null)
            {
                return null;
            }

            if (message.Sender == null)
            {
                message.Sender = new FacebookBotUser { Id = message.Optin?.UserRef };
            }

            var activity = new Activity()
            {
                ChannelId = "facebook",
                Timestamp = new DateTime(),
                Conversation = new ConversationAccount()
                {
                    Id = message.Sender.Id,
                },
                From = new ChannelAccount()
                {
                    Id = message.Sender.Id,
                    Name = message.Sender.Id,
                },
                Recipient = new ChannelAccount()
                {
                    Id = message.Recipient.Id,
                    Name = message.Recipient.Id,
                },
                ChannelData = message,
                Type = ActivityTypes.Event,
                Text = null,
            };

            if (message.Message != null)
            {
                activity.Type = ActivityTypes.Message;
                activity.Text = message.Message.Text;

                if (activity.GetChannelData<FacebookMessage>().Message.IsEcho)
                {
                    activity.Type = ActivityTypes.Event;
                }

                // copy fields like attachments, sticker, quick_reply, nlp, etc.
                activity.ChannelData = message.Message;
                if (message.Message.Attachments != null && message.Message.Attachments.Count > 0)
                {
                    activity.Attachments = HandleMessageAttachments(message.Message);
                }
            }
            else if (message.PostBack != null)
            {
                activity.Type = ActivityTypes.Message;
                activity.Text = message.PostBack.Payload;
            }

            return activity;
        }

        public static List<Attachment> HandleMessageAttachments(Message message)
        {
            var attachmentsList = new List<Attachment>();

            foreach (var facebookAttachment in message.Attachments)
            {
                var attachment = new Attachment
                {
                    ContentUrl = facebookAttachment.Payload.Url.ToString(),
                    ContentType = facebookAttachment.Type,
                };

                attachmentsList.Add(attachment);
            }

            return attachmentsList;
        }

        /// <summary>
        /// Writes the HttpResponse.
        /// </summary>
        /// <param name="response">The httpResponse.</param>
        /// <param name="code">The status code to be written.</param>
        /// <param name="text">The text to be written.</param>
        /// <param name="encoding">The encoding for the text.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task WriteAsync(HttpResponse response, HttpStatusCode code, string text, Encoding encoding, CancellationToken cancellationToken)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            response.ContentType = "text/plain";
            response.StatusCode = (int)code;

            var data = encoding.GetBytes(text);

            await response.Body.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }
    }
}
