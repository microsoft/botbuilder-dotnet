// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.Slack.Model;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Events;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

#if SIGNASSEMBLY
[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Adapters.Slack.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
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
                throw new ArgumentNullException(nameof(activity));
            }

            var message = new NewSlackMessage();

            if (activity.Timestamp != null)
            {
                message.Ts = activity.Timestamp.Value.DateTime.ToString(CultureInfo.InvariantCulture);
            }

            message.Text = activity.Text;

            if (activity.Attachments != null)
            {
                var attachments = new List<SlackAttachment>();

                foreach (var att in activity.Attachments)
                {
                    if (att.Name == "blocks")
                    {
                        message.Blocks = att.Content;
                    }
                    else
                    {
                        var newAttachment = new SlackAttachment()
                        {
                            AuthorName = att.Name,
                            ThumbUrl = new Uri(att.ThumbnailUrl),
                        };
                        attachments.Add(newAttachment);
                    }
                }

                if (attachments.Count > 0)
                {
                    message.Attachments = attachments;
                }
            }

            message.Channel = activity.Conversation.Id;

            if (!string.IsNullOrWhiteSpace(activity.Conversation.Properties["thread_ts"]?.ToString()))
            {
                message.ThreadTs = activity.Conversation.Properties["thread_ts"].ToString();
            }

            // if channelData is specified, overwrite any fields in message object
            if (activity.ChannelData != null)
            {
                message = activity.GetChannelData<NewSlackMessage>();
            }

            // should this message be sent as an ephemeral message
            if (!string.IsNullOrWhiteSpace(message.Ephemeral))
            {
                message.User = activity.Recipient.Id;
            }

            return message;
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
        /// <param name="slackPayload">The payload of the slack event.</param>
        /// <returns>An activity containing the event data.</returns>
        public static Activity PayloadToActivity(InteractionPayload slackPayload)
        {
            if (slackPayload == null)
            {
                throw new ArgumentNullException(nameof(slackPayload));
            }

            var activity = new Activity()
            {
                Timestamp = default,
                ChannelId = "slack",
                Conversation = new ConversationAccount()
                {
                    Id = slackPayload.Channel.id,
                },
                From = new ChannelAccount()
                {
                    Id = slackPayload.Message?.BotId ?? slackPayload.User.id,
                },
                Recipient = new ChannelAccount()
                {
                    Id = null,
                },
                ChannelData = slackPayload,
                Text = null,
                Type = ActivityTypes.Event,
                Value = slackPayload
            };

            if (slackPayload.ThreadTs != null)
            {
                activity.Conversation.Properties["thread_ts"] = slackPayload.ThreadTs;
            }

            if (slackPayload.Actions != null && slackPayload.Actions.Any())
            {
                var action = slackPayload.Actions[0];

                switch (action.Type)
                {
                    case "button":
                        activity.Text = action.Value;
                        break;
                    case "select":
                        activity.Text = slackPayload.Actions[0].SelectedOptions[0]?.Value ?? slackPayload.Actions[0].SelectedOption?.Value;
                        break;
                    case "static_select":
                        activity.Text = slackPayload.Actions[0].SelectedOption.Value;
                        break;
                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(activity.Text))
                {
                    activity.Type = ActivityTypes.Message;
                }
            }

            return activity;
        }

        /// <summary>
        /// Creates an activity based on the slack event data.
        /// </summary>
        /// <param name="eventRequest">The data of the slack event.</param>
        /// <param name="client">The Slack client.</param>
        /// <returns>An activity containing the event data.</returns>
        public static Activity EventToActivity(EventRequest eventRequest, SlackClientWrapper client)
        {
            if (eventRequest == null)
            {
                throw new ArgumentNullException(nameof(eventRequest));
            }

            var innerEvent = eventRequest.Event;

            var activity = new Activity
            {
                Id = innerEvent.EventTs,
                Timestamp = default,
                ChannelId = "slack",
                Conversation =
                    new ConversationAccount()
                    {
                        Id = innerEvent.Channel ?? innerEvent.ChannelId ?? eventRequest.TeamId
                    },
                From = new ChannelAccount()
                {
                    Id = innerEvent.User ?? innerEvent.BotId ?? eventRequest.TeamId
                },
                ChannelData = eventRequest,
                Type = ActivityTypes.Event
            };

            activity.Recipient = new ChannelAccount()
            {
                Id = client.GetBotUserIdentity(activity)
            };

            if (!string.IsNullOrEmpty(innerEvent.ThreadTs))
            {
                activity.Conversation.Properties["thread_ts"] = innerEvent.ThreadTs;
            }

            if (innerEvent.Type == "message" && innerEvent.BotId == null)
            {
                var message = JObject.FromObject(innerEvent).ToObject<MessageEvent>();

                if (message.SubType == null || message.SubType == "file_share")
                {
                    activity.Type = ActivityTypes.Message;
                    activity.Text = message.Text;
                    if (message.AdditionalProperties.ContainsKey("files"))
                    {
                        var attachments = new List<Attachment>();
                        foreach (var attachment in message.AdditionalProperties["files"])
                        {
                            var attachmentProperties = attachment.Value<JObject>().Properties();

                            var contentType = string.Empty;
                            var contentUrl = string.Empty;
                            var name = string.Empty;

                            foreach (var property in attachmentProperties)
                            {
                                if (property.Name == "mimetype")
                                {
                                    contentType = property.Value.ToString();
                                }

                                if (property.Name == "url_private_download")
                                {
                                    contentUrl = property.Value.ToString();
                                }

                                if (property.Name == "name")
                                {
                                    name = property.Value.ToString();
                                }
                            }

                            attachments.Add(new Attachment
                            { 
                                ContentType = contentType,
                                ContentUrl = contentUrl,
                                Name = name
                            });
                        }

                        activity.Attachments = attachments;
                    }
                }

                activity.Conversation.Properties["channel_type"] = message.ChannelType;
                activity.Value = innerEvent;
            }
            else
            {
                activity.Name = innerEvent.Type;
                activity.Value = innerEvent;
            }

            return activity;
        }

        /// <summary>
        /// Creates an activity based on a slack event related to a slash command.
        /// </summary>
        /// <param name="commandRequest">The data of the slack command request.</param>
        /// <param name="client">The Slack client.</param>
        /// <returns>An activity containing the event data.</returns>
        public static Activity CommandToActivity(CommandPayload commandRequest, SlackClientWrapper client)
        {
            if (commandRequest == null)
            {
                throw new ArgumentNullException(nameof(commandRequest));
            }

            var activity = new Activity()
            {
                Id = commandRequest.TriggerId,
                Timestamp = default,
                ChannelId = "slack",
                Conversation = new ConversationAccount()
                {
                    Id = commandRequest.ChannelId,
                },
                From = new ChannelAccount()
                {
                    Id = commandRequest.UserId,
                },
                ChannelData = commandRequest,
                Type = ActivityTypes.Event,
                Name = "Command",
                Value = commandRequest.Command
            };

            activity.Recipient = new ChannelAccount()
            {
                Id = client.GetBotUserIdentity(activity)
            };

            activity.Conversation.Properties["team"] = commandRequest.TeamId;

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
    }
}
