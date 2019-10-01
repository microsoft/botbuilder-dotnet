// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Bot.StreamingExtensions.Transport;
using Microsoft.Bot.StreamingExtensions.Transport.WebSockets;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Skills.Internal
{
    /// <summary>
    /// SkillWebSocketsConnector that inherits from the base SkillConnector.
    /// </summary>
    /// <remarks>
    /// Its responsibility is to forward a incoming request to the skill and handle
    /// the responses based on Skill Protocol.
    /// </remarks>
    internal class SkillWebSocketsConnector : SkillConnector
    {
        private readonly IBotTelemetryClient _botTelemetryClient;
        private readonly MicrosoftAppCredentials _serviceClientCredentials;
        private readonly SkillOptions _skillOptions;

        public SkillWebSocketsConnector(SkillOptions skillOptions, MicrosoftAppCredentials serviceClientCredentials, IBotTelemetryClient botTelemetryClient)
        {
            _botTelemetryClient = botTelemetryClient;
            _skillOptions = skillOptions;
            _serviceClientCredentials = serviceClientCredentials;
        }

        public override async Task<SkillTurnResult> ProcessActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            return await ProcessActivityAsync(turnContext, activity, null, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<SkillTurnResult> ProcessActivityAsync(ITurnContext turnContext, Activity activity, SendActivitiesHandler activitiesHandler, CancellationToken cancellationToken)
        {
            var responseHandler = new SkillWebSocketsResponseHandler(turnContext, activitiesHandler, _botTelemetryClient);
            using (var streamingTransportClient = CreateWebSocketClient(responseHandler))
            {
                await ConnectAsync(streamingTransportClient, activity.ChannelId).ConfigureAwait(false);
                await SendActivityAsync(streamingTransportClient, activity, cancellationToken).ConfigureAwait(false);
                if (streamingTransportClient != null && streamingTransportClient.IsConnected)
                {
                    streamingTransportClient.Disconnect();
                }
            }

            // Default to waiting
            var result = new SkillTurnResult(SkillTurnStatus.Waiting);

            // TODO: Find a better way of handling eoc.
            var eocActivity = responseHandler.GetEndOfConversationActivity();
            if (eocActivity != null)
            {
                result.Status = SkillTurnStatus.Complete;
                result.Result = eocActivity.Value;
            }

            return result;
        }

        private static string EnsureWebSocketUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url), "url is empty!");
            }

            const string httpPrefix = "http://";
            const string httpsPrefix = "https://";

            if (url.StartsWith(httpPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return url.Replace(httpPrefix, "ws://");
            }

            if (url.StartsWith(httpsPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return url.Replace(httpsPrefix, "wss://");
            }

            return url;
        }

        private async Task SendActivityAsync(IStreamingTransportClient streamingTransportClient, Activity activity, CancellationToken cancellationToken)
        {
            // set recipient to the skill
            var recipientId = activity.Recipient.Id;
            activity.Recipient.Id = _skillOptions.MsaAppId;

            var stopWatch = new Stopwatch();

            // Serialize the activity and POST to the Skill endpoint
            using (var body = new StringContent(JsonConvert.SerializeObject(activity, SerializationSettings.BotSchemaSerializationSettings), Encoding.UTF8, SerializationSettings.ApplicationJson))
            {
                var request = StreamingRequest.CreatePost(_skillOptions.Endpoint.AbsolutePath, body);

                // set back recipient id to make things consistent
                activity.Recipient.Id = recipientId;

                stopWatch.Start();
                await streamingTransportClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                stopWatch.Stop();
            }

            _botTelemetryClient.TrackEvent(
                "SkillWebSocketTurnLatency",
                new Dictionary<string, string>
                {
                    { "SkillName", _skillOptions.Name },
                    { "SkillEndpoint", _skillOptions.Endpoint.ToString() },
                },
                new Dictionary<string, double>
                {
                    { "Latency", stopWatch.ElapsedMilliseconds },
                });
        }

        private async Task ConnectAsync(IStreamingTransportClient streamingTransportClient, string channelId)
        {
            // acquire AAD token
            var token = await _serviceClientCredentials.GetTokenAsync().ConfigureAwait(false);

            // put AAD token in the header
            var authHeaders = new Dictionary<string, string>()
            {
                { "authorization", $"Bearer {token}" },
                { "channelid", channelId },
            };

            await streamingTransportClient.ConnectAsync(authHeaders).ConfigureAwait(false);
        }

        private IStreamingTransportClient CreateWebSocketClient(RequestHandler responseHandler)
        {
            return new WebSocketClient(
                EnsureWebSocketUrl(_skillOptions.Endpoint.ToString()),
                responseHandler);
        }
    }
}
