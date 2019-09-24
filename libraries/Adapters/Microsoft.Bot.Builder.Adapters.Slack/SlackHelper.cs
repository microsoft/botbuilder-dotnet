// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

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

        /// <summary>
        /// Writes the HttpResponse.
        /// </summary>
        /// <param name="response">The httpResponse.</param>
        /// <param name="code">The status code to be written.</param>
        /// <param name="text">The text to be written.</param>
        /// <param name="encoding">The encoding for the text.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task WriteAsync(HttpResponse response, HttpStatusCode code, string text, Encoding encoding, CancellationToken cancellationToken = default)
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
        /// Creates an activity based on the slack event payload.
        /// </summary>
        /// <param name="slack">The payload of the slack event.</param>
        /// <returns>An activity containing the event data.</returns>
        public static Activity PayloadToActivity(SlackEvent slack)
        {
            if (slack == null)
            {
                return null;
            }

            var activity = new Activity()
            {
                Timestamp = default(DateTime),
                ChannelId = "slack",
                Conversation = new ConversationAccount()
                {
                    Id = slack.ChannelId,
                },
                From = new ChannelAccount()
                {
                    Id = slack.BotId ?? slack.UserId,
                },
                Recipient = new ChannelAccount()
                {
                    Id = null,
                },
                ChannelData = slack,
                Text = null,
                Type = ActivityTypes.Event,
            };

            activity.Conversation.Properties["thread_ts"] = slack.ThreadTS;
            activity.Conversation.Properties["team"] = slack.Team;

            if ((slack.Type == "block_actions" || slack.Type == "interactive_message") && slack.Actions != null)
            {
                activity.Type = ActivityTypes.Message;
                activity.Text = slack.Actions[0];
            }

            return activity;
        }

        /// <summary>
        /// Creates an activity based on the slack event data.
        /// </summary>
        /// <param name="slack">The data of the slack event.</param>
        /// <param name="client">The Slack client.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>An activity containing the event data.</returns>
        public static async Task<Activity> EventToActivityAsync(SlackEvent slack, SlackClientWrapper client, CancellationToken cancellationToken)
        {
            if (slack == null)
            {
                return null;
            }

            var activity = new Activity()
            {
                Id = slack.EventTS,
                Timestamp = default(DateTime),
                ChannelId = "slack",
                Conversation = new ConversationAccount()
                {
                    Id = slack.Channel ?? slack.ChannelId,
                },
                From = new ChannelAccount()
                {
                    Id = slack.BotId ?? slack.UserId,
                },
                Recipient = new ChannelAccount()
                {
                    Id = null,
                },
                ChannelData = slack,
                Text = null,
                Type = ActivityTypes.Event,
            };

            activity.Conversation.Properties["thread_ts"] = slack.ThreadTS;

            if (activity.Conversation.Id == null)
            {
                if (slack.Item != null && slack.ItemChannel != null)
                {
                    activity.Conversation.Id = slack.ItemChannel;
                }
                else
                {
                    activity.Conversation.Id = slack.Team;
                }
            }

            activity.Recipient.Id = await client.GetBotUserByTeamAsync(activity, cancellationToken).ConfigureAwait(false);

            // If this is conclusively a message originating from a user, we'll mark it as such
            if (slack.Type == "message" && slack.SubType == null)
            {
                activity.Type = ActivityTypes.Message;
                activity.Text = slack.Text;
            }

            return activity;
        }

        /// <summary>
        /// Creates an activity based on a slack event related to a slash command.
        /// </summary>
        /// <param name="slack">The data of the slack event.</param>
        /// <param name="client">The Slack client.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>An activity containing the event data.</returns>
        public static async Task<Activity> CommandToActivityAsync(SlackRequestBody slack, SlackClientWrapper client, CancellationToken cancellationToken)
        {
            if (slack == null)
            {
                return null;
            }

            var activity = new Activity()
            {
                Id = slack.TriggerId,
                Timestamp = default(DateTime),
                ChannelId = "slack",
                Conversation = new ConversationAccount()
                {
                    Id = slack.ChannelId,
                },
                From = new ChannelAccount()
                {
                    Id = slack.UserId,
                },
                Recipient = new ChannelAccount()
                {
                    Id = null,
                },
                ChannelData = slack,
                Text = slack.Text,
                Type = ActivityTypes.Event,
            };

            activity.Recipient.Id = await client.GetBotUserByTeamAsync(activity, cancellationToken).ConfigureAwait(false);

            // activity.GetChannelData<NewSlackMessage>().team = slack.TeamId;

            // add the team id to the conversation record
            activity.Conversation.Properties["team"] = slack.TeamId; // activity.GetChannelData<NewSlackMessage>().team;

            return activity;
        }

        /// <summary>
        /// Converts a query string to a dictionary with key-value pairs.
        /// </summary>
        /// <param name="query">The query string to convert.</param>
        /// <returns>A dictionary with the query values.</returns>
        public static Dictionary<string, string> QueryStringToDictionary(string query)
        {
            var values = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(query))
            {
                return values;
            }

            var pairs = query.Replace("+", "%20").Split('&');

            foreach (var p in pairs)
            {
                var pair = p.Split('=');
                var key = pair[0];
                var value = Uri.UnescapeDataString(pair[1]);

                values.Add(key, value);
            }

            return values;
        }

        /// <summary>
        /// Deserializes the request's body as a <see cref="SlackRequestBody"/> object.
        /// </summary>
        /// <param name="requestBody">The query string to convert.</param>
        /// <returns>A dictionary with the query values.</returns>
        public static SlackRequestBody DeserializeBody(string requestBody)
        {
            SlackRequestBody slackBody = null;

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return slackBody;
            }

            // Check if it's a command event
            if (requestBody.Contains("command=%2F"))
            {
                var commandBody = QueryStringToDictionary(requestBody);

                slackBody = JsonConvert.DeserializeObject<SlackRequestBody>(JsonConvert.SerializeObject(commandBody));
            }
            else
            {
                slackBody = JsonConvert.DeserializeObject<SlackRequestBody>(requestBody);
            }

            return slackBody;
        }
    }
}
