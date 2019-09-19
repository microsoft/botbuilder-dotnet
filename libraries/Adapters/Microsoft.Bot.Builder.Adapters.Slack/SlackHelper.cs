// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
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

            if (activity.Attachments != null)
            {
                foreach (var att in activity.Attachments)
                {
                    var newAttachment = new SlackAPI.Attachment()
                    {
                        author_name = att.Name,
                        thumb_url = att.ThumbnailUrl,
                    };
                    message.attachments.Add(newAttachment);
                }
            }

            message.channel = activity.Conversation.Id;

            if (!string.IsNullOrWhiteSpace(activity.Conversation.Properties["thread_ts"]?.ToString()))
            {
                message.ThreadTS = activity.Conversation.Properties["thread_ts"].ToString();
            }

            // if channelData is specified, overwrite any fields in message object
            if (activity.ChannelData != null)
            {
                message = activity.GetChannelData<NewSlackMessage>();
            }

            // should this message be sent as an ephemeral message
            if (!string.IsNullOrWhiteSpace(message.Ephemeral))
            {
                message.user = activity.Recipient.Id;
            }

            if (message.IconUrl != null || !string.IsNullOrWhiteSpace(message.icons?.status_emoji) || !string.IsNullOrWhiteSpace(message.username))
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
        /// <param name="body">The raw body of the request.</param>
        /// <returns>The result of the comparison between the signature in the request and hashed secret.</returns>
        public static bool VerifySignature(string secret, HttpRequest request, string body)
        {
            string baseString;

            var timestamp = request.Headers["X-Slack-Request-Timestamp"];

            object[] signature = { "v0", timestamp.ToString(), body };
            baseString = string.Join(":", signature);

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));

                var hash = string.Concat("v0=", BitConverter.ToString(hashArray).Replace("-", string.Empty)).ToUpperInvariant();

                var retrievedSignature = request.Headers["X-Slack-Signature"].ToString().ToUpperInvariant();

                return hash == retrievedSignature;
            }
        }

        /// <summary>
        /// Converts the 'Event' subobject of the Slack payload into a NewSlackMessage for assigning to ChannelData.
        /// </summary>
        /// <param name="slackEvent">A dynamic payload from the request body sent by Slack.</param>
        /// <returns>A NewSlackMessage with the resulting properties.</returns>
        public static NewSlackMessage GetChannelDataFromSlackEvent(dynamic slackEvent)
        {
            // Convert Slack timestamp format to DateTime
            var eventProperty = slackEvent["event"];
            string[] splitString = eventProperty.ts.ToString().Split('.');
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime().AddSeconds(Convert.ToDouble(splitString[0], CultureInfo.InvariantCulture));

            return new NewSlackMessage()
            {
                type = eventProperty.type ?? null,
                text = eventProperty.text ?? null,
                user = eventProperty.user ?? null,
                ts = dateTime,
                team = eventProperty.team ?? null,
                channel = eventProperty.channel ?? null,
            };
        }
    }
}
