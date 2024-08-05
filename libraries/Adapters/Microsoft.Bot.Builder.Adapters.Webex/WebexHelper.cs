// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Thrzn41.WebexTeams.Version1;

#if SIGNASSEMBLY
[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Adapters.Webex.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
#else
[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Adapters.Webex.Tests")]
#endif

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    internal static class WebexHelper
    {
        /// <summary>
        /// Creates an <see cref="Activity"/> using the body of a request.
        /// </summary>
        /// <param name="payload">The payload obtained from the body of the request.</param>
        /// <param name="identity">The identity of the bot.</param>
        /// <returns>An <see cref="Activity"/> object.</returns>
        public static Activity PayloadToActivity(WebhookEventData payload, Person identity)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var activity = new Activity
            {
                Id = payload.Id,
                Timestamp = default,
                ChannelId = "webex",
                Conversation = new ConversationAccount
                {
                    Id = payload.SpaceMembershipData.SpaceId,
                },
                From = new ChannelAccount
                {
                    Id = payload.ActorId,
                },
                Recipient = new ChannelAccount
                {
                    Id = identity.Id,
                },
                ChannelData = payload,
                Type = ActivityTypes.Event,
            };

            if (payload.MessageData.FileCount > 0)
            {
                activity.Attachments = HandleMessageAttachments(payload.MessageData);
            }

            return activity;
        }

        /// <summary>
        /// Gets a decrypted <see cref="Message"/> by its Id.
        /// </summary>
        /// <param name="payload">The payload obtained from the body of the request.</param>
        /// <param name="decrypterFunc">The function used to decrypt the message.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Message"/> object.</returns>
        public static async Task<Message> GetDecryptedMessageAsync(WebhookEventData payload, Func<string, CancellationToken, Task<Message>> decrypterFunc, CancellationToken cancellationToken)
        {
            if (payload == null)
            {
                return null;
            }

            return await decrypterFunc(payload.MessageData.Id, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts a decrypted <see cref="Message"/> into an <see cref="Activity"/>.
        /// </summary>
        /// <param name="decryptedMessage">The decrypted message obtained from the body of the request.</param>
        /// <param name="identity">The identity of the bot.</param>
        /// <returns>An <see cref="Activity"/> object.</returns>
        public static Activity DecryptedMessageToActivity(Message decryptedMessage, Person identity)
        {
            if (decryptedMessage == null)
            {
                return null;
            }

            var activity = new Activity
            {
                Id = decryptedMessage.Id,
                Timestamp = default,
                ChannelId = "webex",
                Conversation = new ConversationAccount
                {
                    Id = decryptedMessage.SpaceId,
                },
                From = new ChannelAccount
                {
                    Id = decryptedMessage.PersonId,
                    Name = decryptedMessage.PersonEmail,
                },
                Recipient = new ChannelAccount
                {
                    Id = identity.Id,
                },
                Text = !string.IsNullOrEmpty(decryptedMessage.Text) ? decryptedMessage.Text : string.Empty,
                ChannelData = decryptedMessage,
                Type = ActivityTypes.Message,
            };

            // this is the bot speaking
            if (activity.From.Id == identity.Id)
            {
                activity.Type = ActivityTypes.Event;
                activity.Name = "self_message";
            }

            if (decryptedMessage.HasHtml)
            {
                // strip the mention & HTML from the message
                var pattern = new Regex($"^(<p>|<div>)?<spark-mention .*?data-object-id=\"{identity.Id}\".*?>.*?</spark-mention>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (!pattern.IsMatch(decryptedMessage.Html))
                {
                    var encodedId = $"{identity.Id}=";
                    var buffer = Convert.FromBase64String(encodedId);
                    var decodedId = Encoding.ASCII.GetString(buffer);

                    // this should look like ciscospark://us/PEOPLE/<id string>
                    var match = Regex.Match(decodedId, "ciscospark://.*/(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (match.Captures.Count > 0)
                    {
                        pattern = new Regex(
                            $"^(<p>|<div>)?<spark-mention .*?data-object-id=\"{match.Groups[1]}\".*?>.*?</spark-mention>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    }
                }

                var action = pattern.Replace(decryptedMessage.Html, string.Empty);

                // Strip the remaining HTML tags and replace the message text with the HTML version
                var remainingHtml = new Regex("<.*?>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                activity.Text = remainingHtml.Replace(action, string.Empty).Trim();
            }
            else
            {
                var pattern = new Regex("^" + identity.DisplayName + "\\s+");
                activity.Text = pattern.Replace(activity.Text, string.Empty);
            }

            if (decryptedMessage.FileCount > 0)
            {
                activity.Attachments = HandleMessageAttachments(decryptedMessage);
            }

            return activity;
        }

        /// <summary>
        /// Converts a decrypted <see cref="Message"/> related to an attachment action into an <see cref="Activity"/>.
        /// </summary>
        /// <param name="decryptedMessage">The decrypted message obtained from the body of the request.</param>
        /// <param name="identity">The identity of the bot.</param>
        /// <returns>An <see cref="Activity"/> object.</returns>
        public static Activity AttachmentActionToActivity(Message decryptedMessage, Person identity)
        {
            if (decryptedMessage == null)
            {
                return null;
            }

            var serializerSettings = new JsonSerializerSettings { MaxDepth = null };
            var data = JsonConvert.SerializeObject(decryptedMessage, serializerSettings);

            var messageExtraData = JsonConvert.DeserializeObject<AttachmentActionData>(data, serializerSettings);

            var activity = new Activity
            {
                Id = decryptedMessage.Id,
                Timestamp = default,
                ChannelId = "webex",
                Conversation = new ConversationAccount
                {
                    Id = decryptedMessage.SpaceId,
                },
                From = new ChannelAccount
                {
                    Id = decryptedMessage.PersonId,
                    Name = decryptedMessage.PersonEmail,
                },
                Recipient = new ChannelAccount
                {
                    Id = identity.Id,
                },
                Text = !string.IsNullOrEmpty(decryptedMessage.Text) ? decryptedMessage.Text : string.Empty,
                Value = messageExtraData.Inputs,
                ChannelData = decryptedMessage,
                Type = ActivityTypes.Event,
            };

            return activity;
        }

        /// <summary>
        /// Adds the message's files to an attachments list.
        /// </summary>
        /// <param name="message">The message with the files to process.</param>
        /// <returns>A list of attachments containing the message's files.</returns>
        public static List<Schema.Attachment> HandleMessageAttachments(Message message)
        {
            var attachmentsList = new List<Schema.Attachment>();

            var attachment = new Schema.Attachment
            {
                // Currently Webex API takes only one attachment
                ContentUrl = message.FileUris[0].AbsoluteUri,
            };

            attachmentsList.Add(attachment);

            return attachmentsList;
        }
    }
}
