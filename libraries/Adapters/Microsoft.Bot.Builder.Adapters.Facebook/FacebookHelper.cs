// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

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
                throw new ArgumentNullException(nameof(activity));
            }

            var facebookMessage = new FacebookMessage(activity.Conversation.Id, new Message(), "RESPONSE");

            facebookMessage.Message.Text = activity.Text;

            // map these fields to their appropriate place
            if (activity.ChannelData != null)
            {
                facebookMessage = activity.GetChannelData<FacebookMessage>();

                // make sure the quick reply has a type
                if (activity.GetChannelData<FacebookMessage>().Message.QuickReplies.Any())
                {
                    foreach (var reply in facebookMessage.Message.QuickReplies)
                    {
                        if (string.IsNullOrWhiteSpace(reply.ContentType))
                        {
                            reply.ContentType = "text";
                        }
                    }
                }
            }

            if (activity.Attachments != null && activity.Attachments.Count > 0)
            {
                var payload = JsonConvert.DeserializeObject<MessagePayload>(JsonConvert.SerializeObject(
                    activity.Attachments[0].Content,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                    }));
                
                var attach = new FacebookAttachment
                {
                    Type = activity.Attachments[0].ContentType,
                    Payload = payload,
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
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Sender == null && message.OptIn?.UserRef != null)
            {
                message.Sender = new FacebookBotUser { Id = message.OptIn?.UserRef };
            }

            var activity = new Activity()
            {
                ChannelId = "facebook",
                Timestamp = new DateTime(),
                Conversation = new ConversationAccount()
                {
                    Id = message.Sender?.Id,
                },
                From = new ChannelAccount()
                {
                    Id = message.Sender?.Id,
                    Name = message.Sender?.Id,
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

                // copy all fields (like attachments, sticker, quick_reply, nlp, etc.)
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
                    Content = facebookAttachment.Payload,
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
