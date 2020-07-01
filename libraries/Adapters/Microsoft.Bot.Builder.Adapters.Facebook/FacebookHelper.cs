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
    /// <summary>
    /// Helper class for converting between Bot Framework objects and Facebook API objects.
    /// </summary>
    public static class FacebookHelper
    {
        /// <summary>
        /// Converts a Bot Framework activity to a Facebook messenger outbound message ready for the API.
        /// </summary>
        /// <param name="activity">The activity to be converted to Facebook message.</param>
        /// <returns>The resulting message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is null.</exception>
        public static FacebookMessage ActivityToFacebook(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var facebookMessage = new FacebookMessage(activity.Conversation.Id, new Message(), "RESPONSE");

            facebookMessage.Message.Text = activity.Text;

            if (activity.ChannelData != null)
            {
                facebookMessage = activity.GetChannelData<FacebookMessage>();

                if (facebookMessage.SenderAction != null)
                {
                    facebookMessage.Message = null;
                }
                else
                {
                    // make sure the quick reply has a type
                    if (facebookMessage.Message.QuickReplies.Any())
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
            }

            if (activity.Attachments != null && activity.Attachments.Count > 0 && facebookMessage.Message != null)
            {
                var payload = JsonConvert.DeserializeObject<AttachmentPayload>(JsonConvert.SerializeObject(
                    activity.Attachments[0].Content,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                    }));

                var facebookAttachment = new FacebookAttachment
                {
                    Type = activity.Attachments[0].ContentType,
                    Payload = payload,
                };

                facebookMessage.Message.Attachment = facebookAttachment;
            }

            return facebookMessage;
        }

        /// <summary>
        /// Converts a single Facebook messenger message to a Bot Framework activity.
        /// </summary>
        /// <param name="message">The message to be processed.</param>
        /// <returns>An Activity with the result.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
        /// <remarks>A webhook call may deliver more than one message at a time.</remarks>
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
                Timestamp = default,
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

            if (message.PassThreadControl != null)
            {
                activity.Value = message.PassThreadControl;
            }
            else if (message.RequestThreadControl != null)
            {
                activity.Value = message.RequestThreadControl;
            }
            else if (message.TakeThreadControl != null)
            {
                activity.Value = message.TakeThreadControl;
            }

            if (message.Message != null)
            {
                activity.Text = message.Message.Text;
                activity.Type = activity.GetChannelData<FacebookMessage>().Message.IsEcho ? ActivityTypes.Event : ActivityTypes.Message;

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

        /// <summary>
        /// Extracts attachments from a Facebook message.
        /// </summary>
        /// <param name="message">The message to get attachments from.</param>
        /// <returns>A List of the attachments contained within the message.</returns>
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
        /// Writes an HTTP response payload.
        /// </summary>
        /// <param name="response">The HTTP response to write to.</param>
        /// <param name="code">The status code to apply.</param>
        /// <param name="text">The text to be written.</param>
        /// <param name="encoding">The encoding for the text.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/>, <paramref name="text"/>,
        /// or <paramref name="encoding"/> is null.</exception>
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

        /// <summary>
        /// Generates an activity that displays a typing indicator.
        /// </summary>
        /// <param name="recipientId">The ID of the recipient of the message.</param>
        /// <returns>An activity with sender_action equal to "typing_on".</returns>
        public static Activity GenerateTypingActivity(string recipientId)
        {
            var activity = new Activity()
            {
                ChannelId = "facebook",
                Conversation = new ConversationAccount()
                {
                    Id = recipientId,
                },
                ChannelData = new FacebookMessage(recipientId, null, string.Empty),
                Type = ActivityTypes.Message,
                Text = null,
            };

            // we need only the sender action (and the recipient id) to be present in the message
            var message = activity.GetChannelData<FacebookMessage>();
            message.SenderAction = "typing_on";
            message.MessagingType = null;
            message.Sender = null;

            return activity;
        }
    }
}
