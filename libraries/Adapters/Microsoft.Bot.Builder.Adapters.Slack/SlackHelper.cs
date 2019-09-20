// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;

#if SIGNASSEMBLY
[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Adapters.Twilio.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
#else
[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Adapters.Slack.Tests")]
#endif

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
            if (activity == null)
            {
                return null;
            }

            var message = new NewSlackMessage();

            if (activity.Timestamp != null)
            {
                message.ts = activity.Timestamp.Value.DateTime;
            }

            message.text = activity.Text;

            if (activity.Attachments != null)
            {
                var attachments = new List<SlackAPI.Attachment>();

                foreach (var att in activity.Attachments)
                {
                    var newAttachment = new SlackAPI.Attachment()
                    {
                        author_name = att.Name,
                        thumb_url = att.ThumbnailUrl,
                    };
                    attachments.Add(newAttachment);
                }

                message.attachments = attachments;
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
        /// Converts the 'Event' subobject of the Slack payload into a NewSlackMessage.
        /// </summary>
        /// <param name="slackEvent">A dynamic payload from the request body sent by Slack.</param>
        /// <returns>A NewSlackMessage with the resulting properties.</returns>
        public static NewSlackMessage GetMessageFromSlackEvent(dynamic slackEvent)
        {
            if (slackEvent == null)
            {
                return null;
            }

            var eventProperty = slackEvent["event"];

            // Convert Slack timestamp format to DateTime
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

        public static async Task SendHttpResponse(HttpResponse httpResponse, int responseStatusCode, string responseText)
        {
            httpResponse.StatusCode = responseStatusCode;
            httpResponse.ContentType = "text/plain";
            string text = responseText;
            var encoding = Encoding.UTF8;
            var data = encoding.GetBytes(text);
            await httpResponse.Body.WriteAsync(data, 0, data.Length, default).ConfigureAwait(false);
        }
    }
}
