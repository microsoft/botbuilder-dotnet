// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.FunctionalTests
{
    [TestClass]
    [TestCategory("FunctionalTests")]
    [TestCategory("Adapters")]
#if !AUTOMATEDBUILD
    [Ignore]
#endif
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
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("roomId", _roomId),
                new KeyValuePair<string, string>("text", echoGuid)
            });

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userAccessToken);

            await client.PostAsync($"{WebexUrlBase}/messages", data);
        }

        private async Task<string> ReceiveMessageAsync()
        {
            var resultMessage = string.Empty;
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userAccessToken);

            var query = new Dictionary<string, string>()
            {
                ["roomId"] = _roomId,
            };

            var uri = QueryHelpers.AddQueryString($"{WebexUrlBase}/messages", query);

            var response = await client.GetAsync(uri);

            using var content = response.Content;
            var jObject = JObject.Parse(content.ReadAsStringAsync().Result);

            var result = JsonConvert.DeserializeObject<Message[]>(jObject["items"].ToString());

            foreach (var message in result)
            {
                if (message.PersonEmail.Equals(_targetBotEmail, StringComparison.Ordinal))
                {
                    return message.Text;
                }
            }

            return resultMessage;
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
            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                var kvPairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                _userAccessToken = kvPairs["access_token"];
            }
            else
            {
                throw new HttpRequestException($"RefreshAccessToken() failed: response = {response}");
            }
        }

        private void GetEnvironmentVars()
        {
            _roomId = Environment.GetEnvironmentVariable("WebexTestBotWebexRoomId");
            if (string.IsNullOrWhiteSpace(_roomId))
            {
                Assert.Fail("Environment variable 'WebexTestBotWebexRoomId' not found.");
            }

            _refreshToken = Environment.GetEnvironmentVariable("WebexTestBotRefreshToken");
            if (string.IsNullOrWhiteSpace(_refreshToken))
            {
                Assert.Fail("Environment variable 'WebexTestBotRefreshToken' not found.");
            }

            _integrationClientId = Environment.GetEnvironmentVariable("WebexTestBotWebexIntegrationClientId");
            if (string.IsNullOrWhiteSpace(_integrationClientId))
            {
                Assert.Fail("Environment variable 'WebexTestBotWebexIntegrationClientId' not found.");
            }

            _integrationClientSecret = Environment.GetEnvironmentVariable("WebexTestBotWebexIntegrationClientSecret");
            if (string.IsNullOrWhiteSpace(_integrationClientSecret))
            {
                Assert.Fail("Environment variable 'WebexTestBotWebexIntegrationClientSecret' not found.");
            }

            _targetBotEmail = Environment.GetEnvironmentVariable("WebexTestBotWebexBotUserName");
            if (string.IsNullOrWhiteSpace(_targetBotEmail))
            {
                Assert.Fail("Environment variable 'WebexTestBotWebexBotUserName' not found.");
            }
        }
    }
}
