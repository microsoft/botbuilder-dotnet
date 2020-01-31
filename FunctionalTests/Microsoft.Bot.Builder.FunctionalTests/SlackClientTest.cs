// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.FunctionalTests.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.FunctionalTests
{
    [TestClass]
    [TestCategory("FunctionalTests")]
    public class SlackClientTest
    {
        private const string SlackUrlBase = "https://slack.com/api";
        private HttpClient _client;
        private string _slackChannel;
        private string _slackBotToken;

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
            var data = new NameValueCollection
            {
                ["token"] = _slackBotToken,
                ["channel"] = _slackChannel,
                ["text"] = echoGuid,
                ["as_user"] = "true",
            };

            using (var client = new WebClient())
            {
                await client.UploadValuesTaskAsync($"{SlackUrlBase}/chat.postMessage", "POST", data);
            }
        }

        private void GetEnvironmentVars()
        {
            if (string.IsNullOrWhiteSpace(_slackChannel) || string.IsNullOrWhiteSpace(_slackBotToken))
            {
                _slackChannel = Environment.GetEnvironmentVariable("SLACK_CHANNEL");
                if (string.IsNullOrWhiteSpace(_slackChannel))
                {
                    Assert.Inconclusive("Environment variable 'SLACK_CHANNEL' not found.");
                }

                _slackBotToken = Environment.GetEnvironmentVariable("SLACK_BOT_TOKEN");
                if (string.IsNullOrWhiteSpace(_slackBotToken))
                {
                    Assert.Inconclusive("Environment variable 'SLACK_BOT_TOKEN' not found.");
                }
            }
        }
    }
}
