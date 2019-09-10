// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    internal static class SlackHelper
    {
        /// <summary>
        /// Formats a BotBuilder activity into an outgoing Slack message.
        /// </summary>
        /// <param name="activity">A BotBuilder Activity object.</param>
        /// <returns>A Slack message object with {text, attachments, channel, thread ts} as well as any fields found in activity.channelData.</returns>
        public static NewSlackMessage ActivityToSlack(Activity activity)
        {
            var message = new NewSlackMessage();

            if (activity.Timestamp != null)
            {
                message.ts = activity.Timestamp.Value.DateTime;
            }

            message.text = activity.Text;

            foreach (var att in activity.Attachments)
            {
                var newAttachment = new SlackAPI.Attachment()
                {
                    author_name = att.Name,
                    thumb_url = att.ThumbnailUrl,
                };
                message.attachments.Add(newAttachment);
            }

            message.channel = activity.Conversation.Id;

            if (!string.IsNullOrEmpty(activity.Conversation.Properties["thread_ts"].ToString()))
            {
                message.ThreadTS = activity.Conversation.Properties["thread_ts"].ToString();
            }

            // if channelData is specified, overwrite any fields in message object
            if (activity.ChannelData != null)
            {
                try
                {
                    // Try a straight up cast
                    message = activity.ChannelData as NewSlackMessage;
                }
                catch (InvalidCastException)
                {
                    foreach (var property in message.GetType().GetFields())
                    {
                        var name = property.Name;
                        var value = (activity.ChannelData as dynamic)[name];
                        if (value != null)
                        {
                            message.GetType().GetField(name).SetValue(message, value);
                        }
                    }
                }
            }

            // should this message be sent as an ephemeral message
            if (message.Ephemeral != null)
            {
                message.user = activity.Recipient.Id;
            }

            if (message.IconUrl != null || message.icons?.status_emoji != null || message.username != null)
            {
                message.AsUser = false;
            }

            return message;
        }

        /// <summary>
        /// Validates the local secret against the one obtained from the request header.
        /// </summary>
        /// <param name="secret">The local stored secret.</param>
        /// <param name="request">The <see cref="HttpRequest"/> with the signature.</param>
        /// <returns>The result of the comparison between the signature in the request and hashed secret.</returns>
        public static bool VerifySignature(string secret, HttpRequest request)
        {
            var timestamp = request.Headers;
            var body = request.Body;

            object[] signature = { "v0", timestamp.ToString(), body.ToString() };

            var baseString = string.Join(":", signature);

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hash = "v0=" + hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));

                var retrievedSignature = request.Headers["X-Slack-Signature"];

                return hash == retrievedSignature;
            }
        }
    }
}
