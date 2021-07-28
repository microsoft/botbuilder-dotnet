// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.Slack.Model;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Events;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using SlackAPI;
using Attachment = SlackAPI.Attachment;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class SlackClientWrapper
    {
        private const string PostMessageUrl = "https://slack.com/api/chat.postMessage";
        private const string PostEphemeralMessageUrl = "https://slack.com/api/chat.postEphemeral";

        private readonly SlackTaskClient _api;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackClientWrapper"/> class.
        /// Creates a Slack client by supplying the access token.
        /// </summary>
        /// <param name="options">An object containing API credentials, a webhook verification token and other options.</param>
        public SlackClientWrapper(SlackClientWrapperOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(options.SlackVerificationToken) && string.IsNullOrWhiteSpace(options.SlackClientSigningSecret))
            {
                const string message = "****************************************************************************************" +
                                       "* WARNING: Your bot is operating without recommended security mechanisms in place.     *" +
                                       "* Initialize your adapter with a clientSigningSecret parameter to enable               *" +
                                       "* verification that all incoming webhooks originate with Slack:                        *" +
                                       "*                                                                                      *" +
                                       "* var adapter = new SlackAdapter({clientSigningSecret: <my secret from slack>});       *" +
                                       "*                                                                                      *" +
                                       "****************************************************************************************" +
                                       ">> Slack docs: https://api.slack.com/docs/verifying-requests-from-slack";

                throw new InvalidOperationException(message + Environment.NewLine + "Required: include a verificationToken or clientSigningSecret to verify incoming Events API webhooks");
            }

            _api = new SlackTaskClient(options.SlackBotToken);
            LoginWithSlackAsync(default).Wait();
        }

        /// <summary>
        /// Gets the <see cref="SlackClientWrapperOptions"/>.
        /// </summary>
        /// <value>
        /// An object containing API credentials, a webhook verification token and other options.
        /// </value>
        public SlackClientWrapperOptions Options { get; }

        /// <summary>
        /// Gets the user identity.
        /// </summary>
        /// <value>
        /// A string containing the user identity.
        /// </value>
        public string Identity { get; private set; }

        /// <summary>
        /// Wraps Slack API's DeleteMessageAsync method.
        /// </summary>
        /// <param name="channelId">The channel to delete the message from.</param>
        /// <param name="ts">The timestamp of the message.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="DeletedResponse"/> representing the response to deleting the message.</returns>
        public virtual async Task<DeletedResponse> DeleteMessageAsync(string channelId, DateTime ts, CancellationToken cancellationToken)
        {
            return await _api.DeleteMessageAsync(channelId, ts).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Slack API's TestAuthAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The user Id.</returns>
        public virtual async Task<string> TestAuthAsync(CancellationToken cancellationToken)
        {
            var auth = await _api.TestAuthAsync().ConfigureAwait(false);
            return auth.user_id;
        }

        /// <summary>
        /// Wraps Slack API's UpdateAsync method.
        /// </summary>
        /// <param name="ts">The timestamp of the message.</param>
        /// <param name="channelId">The channel to delete the message from.</param>
        /// <param name="text">The text to update with.</param>
        /// <param name="botName">The optional bot name.</param>
        /// <param name="parse">Change how messages are treated.Defaults to 'none'. See https://api.slack.com/methods/chat.postMessage#formatting. </param>
        /// <param name="linkNames">If to find and link channel names and username.</param>
        /// <param name="attachments">The attachments, if any.</param>
        /// <param name="asUser">If the message is being sent as user instead of as a bot.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="UpdateResponse"/> representing the response to the operation.</returns>
        public virtual async Task<SlackResponse> UpdateAsync(string ts, string channelId, string text, string botName = null, string parse = null, bool linkNames = false, Attachment[] attachments = null, bool asUser = false, CancellationToken cancellationToken = default)
        {
            var updateResponse = await _api.UpdateAsync(ts, channelId, text, botName, parse, linkNames, attachments, asUser).ConfigureAwait(false);

            return new SlackResponse()
            {
                Ok = updateResponse.ok,
                Message = new MessageEvent()
                {
                    User = updateResponse.message.user,
                    Type = updateResponse.message.type,
                    Text = updateResponse.message.text
                },
                Channel = updateResponse.channel,
                Ts = updateResponse.ts
            };
        }

        /// <summary>
        /// Validates the local secret against the one obtained from the request header.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> with the signature.</param>
        /// <param name="body">The raw body of the request.</param>
        /// <returns>The result of the comparison between the signature in the request and hashed secret.</returns>
        public virtual bool VerifySignature(HttpRequest request, string body)
        {
            if (request == null || string.IsNullOrWhiteSpace(body))
            {
                return false;
            }

            var timestamp = request.Headers["X-Slack-Request-Timestamp"];

            object[] signature = { "v0", timestamp.ToString(), body };

            var baseString = string.Join(":", signature);

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Options.SlackClientSigningSecret)))
            {
                var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));
                var hash = string.Concat("v0=", BitConverter.ToString(hashArray).Replace("-", string.Empty)).ToUpperInvariant();
                var retrievedSignature = request.Headers["X-Slack-Signature"].ToString().ToUpperInvariant();

                return hash == retrievedSignature;
            }
        }

        /// <summary>
        /// Posts a message to Slack.
        /// </summary>
        /// <param name="message">The message to be posted.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The <see cref="SlackResponse"/> to the posting operation.</returns>
        public virtual async Task<SlackResponse> PostMessageAsync(NewSlackMessage message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                return null;
            }

            var data = new NameValueCollection
            {
                ["token"] = Options.SlackBotToken,
                ["channel"] = message.Channel,
                ["text"] = message.Text,
                ["thread_ts"] = message.ThreadTs,
                ["user"] = message.User,
            };

            if (message.Blocks != null)
            {
                data["blocks"] = JsonConvert.SerializeObject(message.Blocks, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });
            }

            byte[] response;
            using (var client = new WebClient())
            {
                var url = !string.IsNullOrWhiteSpace(message.Ephemeral)
                    ? PostEphemeralMessageUrl
                    : PostMessageUrl;

                response = await client.UploadValuesTaskAsync(url, "POST", data).ConfigureAwait(false);
            }

            return JsonConvert.DeserializeObject<SlackResponse>(Encoding.UTF8.GetString(response));
        }

        /// <summary>
        /// Get the bot user id associated with the team on which an incoming activity originated. This is used internally by the SlackMessageTypeMiddleware to identify direct_mention and mention events.
        /// In single-team mode, this will pull the information from the Slack API at launch.
        /// In multi-team mode, this will use the `getBotUserByTeam` method passed to the constructor to pull the information from a developer-defined source.
        /// </summary>
        /// <param name="activity">An Activity.</param>
        /// <returns>The identity of the bot's user.</returns>
        public virtual string GetBotUserIdentity(Activity activity)
        {
            return Identity;
        }

        /// <summary>
        /// Manages the login to Slack with the given credentials.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task LoginWithSlackAsync(CancellationToken cancellationToken)
        {
            if (Options.SlackBotToken != null)
            {
                Identity = await TestAuthAsync(cancellationToken).ConfigureAwait(false);
            }
            else if (string.IsNullOrWhiteSpace(Options.SlackClientId) ||
                     string.IsNullOrWhiteSpace(Options.SlackClientSecret) ||
                     Options.SlackRedirectUri == null ||
                     Options.SlackScopes.Count == 0)
            {
                throw new InvalidOperationException("Missing Slack API credentials! Provide SlackClientId, SlackClientSecret, scopes and SlackRedirectUri as part of the SlackAdapter options.");
            }
        }
    }
}
