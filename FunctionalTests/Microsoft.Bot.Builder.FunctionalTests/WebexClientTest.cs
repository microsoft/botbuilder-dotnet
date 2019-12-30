// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.FunctionalTests
{
    [TestClass]
    [TestCategory("FunctionalTests")]
    public class WebexClientTest
    {        
        private const string WebexUrlBase = "https://api.ciscospark.com/v1/messages";
        private string _targetBotEmail;
        private string _roomId;
        private string _personalAccessToken;

        [TestMethod]
        public async Task SendAndReceiveWebexMessageShouldSucceed()
        {
            GetEnvironmentVars();
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
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + _personalAccessToken;
                await client.UploadValuesTaskAsync(WebexUrlBase, "POST", data);
            }
        }

        private async Task<string> ReceiveMessageAsync()
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + _personalAccessToken;

                client.QueryString.Add("roomId", _roomId);

                var response = await client.DownloadStringTaskAsync(new Uri(WebexUrlBase));
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

        private void GetEnvironmentVars()
        {
            if (string.IsNullOrWhiteSpace(_roomId) || string.IsNullOrWhiteSpace(_personalAccessToken) || string.IsNullOrWhiteSpace(_targetBotEmail))
            {
                _roomId = Environment.GetEnvironmentVariable("WEBEX_ROOM_ID");
                if (string.IsNullOrWhiteSpace(_roomId))
                {
                    throw new Exception("Environment variable 'WEBEX_ROOM_ID' not found.");
                }

                _personalAccessToken = Environment.GetEnvironmentVariable("PERSONAL_ACCESS_TOKEN");
                if (string.IsNullOrWhiteSpace(_personalAccessToken))
                {
                    throw new Exception("Environment variable 'PERSONAL_ACCESS_TOKEN' not found.");
                }

                _targetBotEmail = Environment.GetEnvironmentVariable("Webex-BotUserName");
                if (string.IsNullOrWhiteSpace(_targetBotEmail))
                {
                    throw new Exception("Environment variable 'Webex-BotUserName' not found.");
                }
            }
        }
    }
}
