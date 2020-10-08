// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.FunctionalTests
{
    [TestClass]
    [TestCategory("FunctionalTests")]
    [TestCategory("Adapters")]
#if !AUTOMATEDBUILD
    [Ignore]
#endif
    public class FacebookChatTests
    {
        private const string FacebookUrlBase = "https://graph.facebook.com/v5.0";
        private string _appSecret;
        private string _accessToken;
        private string _botEndpoint;
        private string _senderId;

        [TestMethod]
        public async Task SendAndReceiveFacebookMessageShouldSucceed()
        {
            GetEnvironmentVars();

            var echoGuid = Guid.NewGuid().ToString();
            await SendMessageAsync(echoGuid);

            var response = await ReceiveMessageAsync();

            Assert.AreEqual($"Echo: {echoGuid}", response);
        }

        private async Task SendMessageAsync(string echoGuid)
        {            
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                var bodyMessage = CreateBodyMessage(echoGuid);
                var hubSignature = CreateHubSignature(bodyMessage);

                request.Headers.Add("user-agent", "facebookexternalua");
                request.Headers.Add("x-hub-signature", hubSignature);
                request.Content = new StringContent(bodyMessage, Encoding.UTF8, "application/json");
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(_botEndpoint);

                await client.SendAsync(request);
            }            
        }

        private async Task<string> ReceiveMessageAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

            var retrieveMessagesUri = $"{FacebookUrlBase}/me/conversations?fields=messages{{message}}&user_id={_senderId}&access_token={_accessToken}";
            var result = string.Empty;

            using (var client = new HttpClient())
            {
                var httpResponse = await client.GetAsync(retrieveMessagesUri);
                var response = httpResponse.Content.ReadAsStringAsync().Result;

                try
                {
                    var messages = JObject.Parse(response);
                    result = messages.SelectToken("data[0].messages.data[0].message").ToString();
                }
                catch
                {
                    result = response;
                }
            }

            return result;
        }

        private string CreateHubSignature(string bodyMessage)
        {
            var hashResult = string.Empty;

            using (var hmac = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(_appSecret)))
            {
                hmac.Initialize();
                var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(bodyMessage));
                var hash = $"SHA1={BitConverter.ToString(hashArray).Replace("-", string.Empty)}";

                hashResult = hash;
            }

            return hashResult;
        }

        private string CreateBodyMessage(string echoGuid)
        {
            JObject sender = new JObject();
            sender["id"] = _senderId;

            JObject recipient = new JObject();

            JObject message = new JObject();
            message["text"] = echoGuid;

            JObject messagingObj = new JObject();
            messagingObj["sender"] = sender;
            messagingObj["recipient"] = recipient;
            messagingObj["message"] = message;

            JArray messaging = new JArray();
            messaging.Add(messagingObj);

            JObject entryObj = new JObject();
            entryObj["messaging"] = messaging;

            JArray entry = new JArray();
            entry.Add(entryObj);

            JObject body = new JObject();
            body["object"] = "page";
            body["entry"] = entry;

            return body.ToString();
        }

        private void GetEnvironmentVars()
        {
            if (string.IsNullOrWhiteSpace(_appSecret) || string.IsNullOrWhiteSpace(_accessToken) || string.IsNullOrWhiteSpace(_botEndpoint) || string.IsNullOrWhiteSpace(_senderId))
            {
                _appSecret = Environment.GetEnvironmentVariable("FacebookTestBotFaceBookAppSecret");
                if (string.IsNullOrWhiteSpace(_appSecret))
                {
                    Assert.Fail("Environment variable 'FacebookTestBotFaceBookAppSecret' not found.");
                }

                _accessToken = Environment.GetEnvironmentVariable("FacebookTestBotFacebookAccessToken");
                if (string.IsNullOrWhiteSpace(_accessToken))
                {
                    Assert.Fail("Environment variable 'FacebookTestBotFacebookAccessToken' not found.");
                }

                _botEndpoint = Environment.GetEnvironmentVariable("BOT_ENDPOINT");
                if (string.IsNullOrWhiteSpace(_botEndpoint))
                {
                    Assert.Fail("Environment variable 'BOT_ENDPOINT' not found.");
                }

                _senderId = Environment.GetEnvironmentVariable("FacebookTestBotSenderId");
                if (string.IsNullOrWhiteSpace(_senderId))
                {
                    Assert.Fail("Environment variable 'FacebookTestBotSenderId' not found.");
                }
            }
        }
    }
}
