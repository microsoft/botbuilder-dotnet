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
    public class FacebookChatTests
    {
        private string _appSecret;
        private string _accessToken;
        private string _botEndpoint;
        private string _senderId;

        [TestMethod]
        public async Task SendAndReceiveFacebookMessageShouldSucceed()
        {
            GetEnvironmentVars();
            string echoGuid = Guid.NewGuid().ToString();
            await SendMessageAsync(echoGuid);
            string response = await ReceiveMessageAsync();

            Assert.AreEqual($"Echo: {echoGuid}", response);
        }

        private async Task SendMessageAsync(string echoGuid)
        {
            string bodyMessage = CreateBodyMessage(echoGuid);
            string hubSignature = CreateHubSignature(bodyMessage);
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.Headers.Add("user-agent", "facebookexternalua");
            request.Headers.Add("x-hub-signature", hubSignature);
            request.Content = new StringContent(bodyMessage, Encoding.UTF8, "application/json");
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_botEndpoint);

            await client.SendAsync(request);
        }

        private async Task<string> ReceiveMessageAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
            string retrieveMessgesUri = $"https://graph.facebook.com/v5.0/me/conversations?fields=messages{{message}}&user_id={_senderId}&access_token={_accessToken}";
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(retrieveMessgesUri);
            var httpResponse = await client.GetAsync(retrieveMessgesUri);
            var response = httpResponse.Content.ReadAsStringAsync().Result;
            JObject messages = JObject.Parse(response);
            var result = messages.SelectToken("data[0].messages.data[0].message").ToString();

            return result;
        }

        private string CreateHubSignature(string bodyMessage)
        {
            string hashResult;

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
                _appSecret = Environment.GetEnvironmentVariable("FacebookAppSecret");
                if (string.IsNullOrWhiteSpace(_appSecret))
                {
                    throw new Exception("Environment variable 'FacebookAppSecret' not found.");
                }

                _accessToken = Environment.GetEnvironmentVariable("FacebookAccessToken");
                if (string.IsNullOrWhiteSpace(_accessToken))
                {
                    throw new Exception("Environment variable 'FacebookAccessToken' not found.");
                }

                _botEndpoint = Environment.GetEnvironmentVariable("BotEndpoint");
                if (string.IsNullOrWhiteSpace(_botEndpoint))
                {
                    throw new Exception("Environment variable 'BotEndpoint' not found.");
                }

                _senderId = Environment.GetEnvironmentVariable("UserId");
                if (string.IsNullOrWhiteSpace(_senderId))
                {
                    throw new Exception("Environment variable 'UserId' not found.");
                }
            }
        }
    }
}
