// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Security;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// A helper class to create Activities and Twilio messages.
    /// </summary>
    internal static class TwilioHelper
    {
        /// <summary>
        /// Formats a BotBuilder activity into an outgoing Twilio SMS message.
        /// </summary>
        /// <param name="activity">A BotBuilder Activity object.</param>
        /// <param name="twilioAdapterOptions">The twilio adapter options.</param>
        /// <returns>A Message's options object with {body, from, to, mediaUrl}.</returns>
        public static CreateMessageOptions ActivityToTwilio(Activity activity, ITwilioAdapterOptions twilioAdapterOptions)
        {
            var mediaUrls = new List<Uri>();
            if (activity.Attachments != null)
            {
                mediaUrls.AddRange(activity.Attachments.Select(attachment => new Uri(attachment.ContentUrl)));
            }

            var messageOptions = new CreateMessageOptions(activity.Conversation.Id)
            {
                ApplicationSid = activity.Conversation.Id,
                From = twilioAdapterOptions.TwilioNumber,
                Body = activity.Text,
                MediaUrl = mediaUrls,
            };

            return messageOptions;
        }

        /// <summary>
        /// Processes a HTTP request into an Activity.
        /// </summary>
        /// <param name="httpRequest">A httpRequest object.</param>
        /// <param name="twilioAdapterOptions">The twilio adapter options.</param>
        /// <returns>The Activity obtained from the httpRequest object.</returns>
        public static Activity RequestToActivity(HttpRequest httpRequest, ITwilioAdapterOptions twilioAdapterOptions)
        {
            Dictionary<string, string> body;
            using (var bodyStream = new StreamReader(httpRequest.Body))
            {
                body = QueryStringToDictionary(bodyStream.ReadToEnd());
            }

            ValidateRequest(httpRequest, body, twilioAdapterOptions);

            var twilioMessage = JsonConvert.DeserializeObject<TwilioMessage>(JsonConvert.SerializeObject(body));

            return new Activity()
            {
                Id = twilioMessage.MessageSid,
                Timestamp = DateTime.UtcNow,
                ChannelId = Channels.Twilio,
                Conversation = new ConversationAccount()
                {
                    Id = twilioMessage.From,
                },
                From = new ChannelAccount()
                {
                    Id = twilioMessage.From,
                },
                Recipient = new ChannelAccount()
                {
                    Id = twilioMessage.To,
                },
                Text = twilioMessage.Body,
                ChannelData = twilioMessage,
                Type = ActivityTypes.Message,
                Attachments = int.TryParse(twilioMessage.NumMedia, out var numMediaResult) && numMediaResult > 0 ? GetMessageAttachments(numMediaResult, body) : null,
            };
        }

        private static void ValidateRequest(HttpRequest httpRequest, Dictionary<string, string> body, ITwilioAdapterOptions twilioAdapterOptions)
        {
            var twilioSignature = httpRequest.Headers["x-twilio-signature"];
            var validationUrl = twilioAdapterOptions.ValidationUrl ?? (httpRequest.Headers["x-forwarded-proto"][0] ?? httpRequest.Protocol + "://" + httpRequest.Host + httpRequest.Path);
            var requestValidator = new RequestValidator(twilioAdapterOptions.AuthToken);
            if (!requestValidator.Validate(validationUrl, body, twilioSignature))
            {
                throw new AuthenticationException("Request does not match provided signature");
            }
        }

        /// <summary>
        /// Extracts attachments (if any) from a twilio message and returns them in an Attachments array.
        /// </summary>
        /// <param name="numMedia">The number of media items to pull from the message body.</param>
        /// <param name="message">A dictionary containing the twilio message elements.</param>
        /// <returns>An Attachments array with the converted attachments.</returns>
        private static List<Attachment> GetMessageAttachments(int numMedia, Dictionary<string, string> message)
        {
            var attachments = new List<Attachment>();
            for (var i = 0; i < numMedia; i++)
            {
                var attachment = new Attachment()
                {
                    ContentType = message[$"MediaContentType{i}"],
                    ContentUrl = message[$"MediaUrl{i}"],
                };
                attachments.Add(attachment);
            }

            return attachments;
        }

        /// <summary>
        /// Converts a query string to a dictionary with key-value pairs.
        /// </summary>
        /// <param name="query">The query string to convert.</param>
        /// <returns>A dictionary with the query values.</returns>
        private static Dictionary<string, string> QueryStringToDictionary(string query)
        {
            var pairs = query.Replace("+", "%20").Split('&');
            var values = new Dictionary<string, string>();

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
