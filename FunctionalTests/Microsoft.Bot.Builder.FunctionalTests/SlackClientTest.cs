// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.FunctionalTests.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.FunctionalTests
{
    [TestClass]
    [TestCategory("FunctionalTests")]
    [TestCategory("Adapters")]
#if !AUTOMATEDBUILD
    [Ignore]
#endif
    public class SlackClientTest
    {
        private const string SlackUrlBase = "https://slack.com/api";
        private HttpClient _client;
        private string _slackChannel;
        private string _slackBotToken;
        private string _slackClientSigningSecret;
        private string _slackVerificationToken;
        private string _botName;

        [TestMethod]
        public async Task SendAndReceiveSlackMessageShouldSucceed()
        {
            GetEnvironmentVars();
            var echoGuid = Guid.NewGuid().ToString();
            await SendMessageAsync(echoGuid);

            var response = await ReceiveMessageAsync();

            Assert.AreEqual($"Echo: {echoGuid}", response);
        }

        private async Task<string> ReceiveMessageAsync()
        {
            var lastMessage = string.Empty;
            var i = 0;

            while (!lastMessage.Contains("Echo") && i < 60)
            {
                _client = new HttpClient();
                var requestUri = $"{SlackUrlBase}/conversations.history?token={_slackBotToken}&channel={_slackChannel}";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(requestUri),
                };

                var httpResponse = await _client.SendAsync(request);

                var response = httpResponse.Content.ReadAsStringAsync().Result;
                lastMessage = JsonConvert.DeserializeObject<SlackHistoryRetrieve>(response).Messages[0].Text;

                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                i++;
            }

            return lastMessage;
        }

        private async Task SendMessageAsync(string echoGuid)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                var message = CreateMessage(echoGuid);
                var hubSignature = CreateHubSignature(message, timestamp);

                request.Headers.Add("X-Slack-Request-Timestamp", timestamp);
                request.Headers.Add("X-Slack-Signature", hubSignature);
                request.Content = new StringContent(message, Encoding.UTF8, "application/json");
                request.Method = HttpMethod.Post;

                request.RequestUri = new Uri($"https://{_botName}.azurewebsites.net/api/messages");

                var response = await client.SendAsync(request);
            }
        }

        private string CreateHubSignature(string message, string timestamp)
        {
            var hashResult = string.Empty;
            object[] signature = { "v0", timestamp, message };
            var baseString = string.Join(":", signature);

            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(_slackClientSigningSecret)))
            {
                var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));
                var hash = string.Concat("v0=", BitConverter.ToString(hashArray).Replace("-", string.Empty)).ToUpperInvariant();

                hashResult = hash;
            }

            return hashResult;
        }

        private string CreateMessage(string echoGuid)
        {
            var message = new JObject
            {
                ["token"] = _slackVerificationToken,
                ["team_id"] = "teamId",
                ["api_app_id"] = "apiAppId"
            };

            var slackEvent = new JObject
            {
                ["client_msg_id"] = "client_msg_id",
                ["type"] = "message",
                ["text"] = echoGuid,
                ["user"] = "userId",
                ["channel"] = _slackChannel,
                ["channel_type"] = "im"
            };

            message["event"] = slackEvent;
            message["type"] = "event_callback";

            return message.ToString();
        }

        private void GetEnvironmentVars()
        {
            if (string.IsNullOrWhiteSpace(_slackChannel) || string.IsNullOrWhiteSpace(_slackBotToken) || string.IsNullOrWhiteSpace(_slackClientSigningSecret) || string.IsNullOrWhiteSpace(_slackVerificationToken) || string.IsNullOrWhiteSpace(_botName))
            {
                _slackChannel = Environment.GetEnvironmentVariable("SlackChannel");
                if (string.IsNullOrWhiteSpace(_slackChannel))
                {
                    Assert.Fail("Environment variable 'SlackChannel' not found.");
                }

                _slackBotToken = Environment.GetEnvironmentVariable("SlackBotToken");
                if (string.IsNullOrWhiteSpace(_slackBotToken))
                {
                    Assert.Fail("Environment variable 'SlackBotToken' not found.");
                }

                _slackClientSigningSecret = Environment.GetEnvironmentVariable("SlackClientSigningSecret");
                if (string.IsNullOrWhiteSpace(_slackClientSigningSecret))
                {
                    Assert.Fail("Environment variable 'SlackClientSigningSecret' not found.");
                }

                _slackVerificationToken = Environment.GetEnvironmentVariable("SlackVerificationToken");
                if (string.IsNullOrWhiteSpace(_slackVerificationToken))
                {
                    Assert.Fail("Environment variable 'SlackVerificationToken' not found.");
                }

                _botName = Environment.GetEnvironmentVariable("BotName");
                if (string.IsNullOrWhiteSpace(_botName))
                {
                    Assert.Fail("Environment variable 'BotName' not found.");
                }
            }
        }
    }
}
