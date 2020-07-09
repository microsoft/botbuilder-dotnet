// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Thrzn41.WebexTeams;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    /// <summary>
    /// A client for interacting with the Webex Teams API.
    /// </summary>
    public class WebexClientWrapper
    {
        private const string MessageUrl = "https://api.ciscospark.com/v1/messages";
        private const string ActionsUrl = "https://api.ciscospark.com/v1/attachment/actions";
        private const string SparkSignature = "x-spark-signature";

        private readonly TeamsAPIClient _api;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebexClientWrapper"/> class.
        /// Creates a Webex Client Wrapper. See <see cref="WebexClientWrapperOptions"/> for a full definition of the allowed parameters.
        /// </summary>
        /// <param name="options">An object containing API credentials, a webhook verification token and other options.</param>
        public WebexClientWrapper(WebexClientWrapperOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(Options.WebexAccessToken))
            {
                throw new ArgumentException(nameof(options.WebexAccessToken));
            }

            if (Options.WebexPublicAddress == null)
            {
                throw new ArgumentException(nameof(options.WebexPublicAddress));
            }

            _api = TeamsAPI.CreateVersion1Client(Options.WebexAccessToken);
        }

        /// <summary>
        /// Gets the options collection for the adapter.
        /// </summary>
        /// <value>A WebexClientWrapperOptions class exposing properties for each of the available options.</value>
        public WebexClientWrapperOptions Options { get; }

        /// <summary>
        /// Validates the local secret against the one obtained from the request header.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> with the signature.</param>
        /// <param name="jsonPayload">The serialized payload to be use for comparison.</param>
        /// <returns>The result of the comparison between the signature in the request and hashed json.</returns>
        public virtual bool ValidateSignature(HttpRequest request, string jsonPayload)
        {
            var signature = request.Headers.ContainsKey(SparkSignature)
                ? request.Headers[SparkSignature].ToString().ToUpperInvariant()
                : throw new InvalidOperationException($"HttpRequest is missing \"{SparkSignature}\"");

#pragma warning disable CA5350 // Webex API uses SHA1 as cryptographic algorithm.
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(Options.WebexSecret)))
            {
                var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(jsonPayload));
                var hash = BitConverter.ToString(hashArray).Replace("-", string.Empty).ToUpperInvariant();

                return signature == hash;
            }
#pragma warning restore CA5350 // Webex API uses SHA1 as cryptographic algorithm.
        }

        /// <summary>
        /// Wraps Webex API's CreateMessageAsync method.
        /// </summary>
        /// <param name="recipient">Target id of the message.</param>
        /// <param name="text">Text of the message.</param>
        /// <param name="files">List of files attached to the message.</param>
        /// <param name="messageType">Type of message. It can be Text or Markdown.</param>
        /// <param name="target">Target for the message.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The created message id.</returns>
        public virtual async Task<string> CreateMessageAsync(string recipient, string text, IList<Uri> files = null, MessageTextType messageType = MessageTextType.Text, MessageTarget target = MessageTarget.PersonId, CancellationToken cancellationToken = default)
        {
            var webexResponse = await _api.CreateMessageAsync(recipient, text, files, target, messageType, cancellationToken: cancellationToken).ConfigureAwait(false);

            return webexResponse.Data.Id;
        }

        /// <summary>
        /// Wraps Webex API's DeleteMessageAsync method.
        /// </summary>
        /// <param name="messageId">The id of the message to be deleted.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task DeleteMessageAsync(string messageId, CancellationToken cancellationToken)
        {
            await _api.DeleteMessageAsync(messageId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a message with attachments.
        /// </summary>
        /// <param name="recipient">PersonId, email or roomId of the message.</param>
        /// <param name="text">Text of the message.</param>
        /// <param name="attachments">List of attachments attached to the message.</param>
        /// <param name="messageType">Type of the message. It can be Text or Markdown.</param>
        /// <param name="target">Target for the message.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The created message id.</returns>
        public virtual async Task<string> CreateMessageWithAttachmentsAsync(string recipient, string text, IList<Attachment> attachments, MessageTextType messageType = MessageTextType.Text, MessageTarget target = MessageTarget.PersonId, CancellationToken cancellationToken = default)
        {
            Message result;

            var attachmentsContent = new List<object>();

            foreach (var attach in attachments)
            {
                attachmentsContent.Add(attach.Content);
            }

            var request = new WebexMessageRequest
            {
                RoomId = target == MessageTarget.SpaceId ? recipient : null,
                ToPersonId = target == MessageTarget.SpaceId ? null : recipient,
                Text = text ?? string.Empty,
                Attachments = attachmentsContent.Count > 0 ? attachmentsContent : null,
            };

            var http = (HttpWebRequest)WebRequest.Create(new Uri(MessageUrl));
            http.PreAuthenticate = true;
            http.Headers.Add("Authorization", "Bearer " + Options.WebexAccessToken);
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";

            var parsedContent = JsonConvert.SerializeObject(request);
            var encoding = new ASCIIEncoding();
            var bytes = encoding.GetBytes(parsedContent);

            var newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = await http.GetResponseAsync().ConfigureAwait(false);

            var stream = response.GetResponseStream();

            using (var sr = new StreamReader(stream))
            {
                var content = await sr.ReadToEndAsync().ConfigureAwait(false);
                result = JsonConvert.DeserializeObject<Message>(content);
            }

            return result.Id;
        }

        /// <summary>
        /// Shows details for an attachment action, by ID.
        /// </summary>
        /// <param name="actionId">An unique identifier for the attachment action.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The attachment action details.</returns>
        public virtual async Task<Message> GetAttachmentActionAsync(string actionId, CancellationToken cancellationToken)
        {
            Message result;

            var url = $"{ActionsUrl}/{actionId}";

            var http = (HttpWebRequest)WebRequest.Create(new Uri(url));
            http.PreAuthenticate = true;
            http.Headers.Add("Authorization", "Bearer " + Options.WebexAccessToken);
            http.Method = "GET";

            var response = await http.GetResponseAsync().ConfigureAwait(false);

            var stream = response.GetResponseStream();

            using (var sr = new StreamReader(stream))
            {
                var content = await sr.ReadToEndAsync().ConfigureAwait(false);
                result = JsonConvert.DeserializeObject<Message>(content);
            }

            return result;
        }

        /// <summary>
        /// Wraps Webex API's GetMeAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The <see cref="Person"/> object associated with the bot.</returns>
        public virtual async Task<Person> GetMeAsync(CancellationToken cancellationToken)
        {
            var resultPerson = await _api.GetMeAsync(cancellationToken).ConfigureAwait(false);
            return resultPerson.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's GetMessageAsync method.
        /// </summary>
        /// <param name="messageId">Id of the message to be recovered.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The message's data.</returns>
        public virtual async Task<Message> GetMessageAsync(string messageId, CancellationToken cancellationToken)
        {
            var message = await _api.GetMessageAsync(messageId, cancellationToken).ConfigureAwait(false);

            return message.GetData(false);
        }
    }
}
