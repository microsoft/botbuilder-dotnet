// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.FunctionalTests
{
    [TestClass]
    [TestCategory("FunctionalTests")]
    [TestCategory("Adapters")]
    public class WebexClientTest
    {        
        private const string WebexUrlBase = "https://api.ciscospark.com/v1";
        private string _targetBotEmail;
        private string _roomId;
        private string _userAccessToken;
        private string _refreshToken;
        private string _integrationClientId;
        private string _integrationClientSecret;

        [TestMethod]
        public async Task SendAndReceiveWebexMessageShouldSucceed()
        {
            GetEnvironmentVars();
            await RefreshAccessToken();
            var echoGuid = Guid.NewGuid().ToString();
            await SendMessageAsync(echoGuid);

            System.Threading.Thread.Sleep(60000);

            var response = await ReceiveMessageAsync();

            Assert.AreEqual($"Echo: {echoGuid}", response);
        }

        private async Task SendMessageAsync(string echoGuid)
        {
            var data = new NameValueCollection
            {
                ["roomId"] = _roomId,
                ["text"] = echoGuid,
            };

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + _userAccessToken;
                await client.UploadValuesTaskAsync($"{WebexUrlBase}/messages", "POST", data);
            }
        }

        private async Task<string> ReceiveMessageAsync()
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + _userAccessToken;

                client.QueryString.Add("roomId", _roomId);

                var response = await client.DownloadStringTaskAsync(new Uri($"{WebexUrlBase}/messages"));
                var jObject = JObject.Parse(response);

                var result = JsonConvert.DeserializeObject<Message[]>(jObject["items"].ToString());

                string resultMessage = string.Empty;
                foreach (var message in result)
                {
                    if (message.PersonEmail.Equals(_targetBotEmail))
                    {
                        return message.Text;
                    }
                }

                return resultMessage;
            }
        }

        private async Task RefreshAccessToken()
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("grant_type", "refresh_token");
            parameters.Add("client_id", _integrationClientId);
            parameters.Add("client_secret", _integrationClientSecret);
            parameters.Add("refresh_token", _refreshToken);
            
            var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, $"{WebexUrlBase}/access_token")
            {
                Content = new FormUrlEncodedContent(parameters)
            };

            var httpResponse = await client.SendAsync(request);
            var response = httpResponse.Content.ReadAsStringAsync().Result;
            var kvPairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

            _userAccessToken = kvPairs["access_token"];
        }

        private void GetEnvironmentVars()
        {
            _roomId = Environment.GetEnvironmentVariable("WEBEX_ROOM_ID");
            if (string.IsNullOrWhiteSpace(_roomId))
            {
                Assert.Inconclusive("Environment variable 'WEBEX_ROOM_ID' not found.");
            }

            _refreshToken = Environment.GetEnvironmentVariable("REFRESH_TOKEN");
            if (string.IsNullOrWhiteSpace(_refreshToken))
            {
                Assert.Inconclusive("Environment variable 'REFRESH_TOKEN' not found.");
            }

            _integrationClientId = Environment.GetEnvironmentVariable("WebexIntegrationClientId");
            if (string.IsNullOrWhiteSpace(_integrationClientId))
            {
                Assert.Inconclusive("Environment variable 'WebexIntegrationClientId' not found.");
            }

            _integrationClientSecret = Environment.GetEnvironmentVariable("INTEGRATION_CLIENT_SECRET");
            if (string.IsNullOrWhiteSpace(_integrationClientSecret))
            {
                Assert.Inconclusive("Environment variable 'INTEGRATION_CLIENT_SECRET' not found.");
            }

            _targetBotEmail = Environment.GetEnvironmentVariable("WebexBotUserName");
            if (string.IsNullOrWhiteSpace(_targetBotEmail))
            {
                Assert.Inconclusive("Environment variable 'WebexBotUserName' not found.");
            }
        }
    }
}
