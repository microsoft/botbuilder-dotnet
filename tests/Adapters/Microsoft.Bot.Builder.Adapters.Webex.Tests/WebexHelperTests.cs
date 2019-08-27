// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Thrzn41.WebexTeams;
using Thrzn41.WebexTeams.Version1;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Webex.Tests
{
    public class WebexHelperTests
    {
        [Fact]
        public void PayloadToActivity_Should_Return_Null_With_Null_Payload()
        {
            Assert.Null(WebexHelper.PayloadToActivity(null));
        }

        [Fact]
        public void PayloadToActivity_Should_Return_Activity()
        {
            var seralizedPayload =
                "{\"id\":\"id\",\"name\":\"name\",\"resource\":\"messages\",\"event\":\"created\",\"filter\":null,\"orgId\":\"orgId\",\"createdBy\":\"creator_id\",\"appId\":\"app_id\",\"ownedBy\":\"creator\",\"status\":\"active\",\"actorId\":\"actor_id\",\"targetUrl\":\"https://contoso.com/api/messages\",\"created\":\"2019-01-01T00:00:00.096Z\",\"data\":{\"id\":\"id\",\"roomId\":\"room_id\",\"roomType\":\"direct\",\"personId\":\"person_id\",\"personEmail\":\"person@email.com\",\"created\":\"2019-01-01T00:00:00.534Z\"}}";
            var payload = JsonConvert.DeserializeObject<WebhookEventData>(seralizedPayload);
            var serializedPerson = "{\"id\":\"person_id\"}";
            WebexHelper.Identity = JsonConvert.DeserializeObject<Person>(serializedPerson);

            Assert.NotNull(WebexHelper.PayloadToActivity(payload));
        }

        [Fact]
        public void ValidateSignature_Should_Fail_With_Missing_Signature()
        {
            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(string.Empty);

            Assert.Throws<Exception>(() =>
            {
                WebexHelper.ValidateSignature("test_secret", httpRequest.Object, "{}");
            });
        }

        [Fact]
        public void ValidateSignature_Should_Return_False()
        {
            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns("wrong_signature");
            httpRequest.Object.Body = Stream.Null;

            Assert.False(WebexHelper.ValidateSignature("test_secret", httpRequest.Object, "{}"));
        }

        [Fact]
        public async void GetDecryptedMessage_Should_Return_Null_With_Null_Payload()
        {
            Assert.Null(await WebexHelper.GetDecryptedMessage(null, null));
        }

        [Fact]
        public void DecryptedMessageToActivityAsync_Should_Return_Null_With_Null_Message()
        {
            Assert.Null(WebexHelper.DecryptedMessageToActivity(null));
        }

        [Fact]
        public void DecryptedMessageToActivity_Should_Return_Activity_Type_SelfMessage()
        {
            var serializedPerson = "{\"id\":\"person_id\"}";
            WebexHelper.Identity = JsonConvert.DeserializeObject<Person>(serializedPerson);

            var message =
                JsonConvert.DeserializeObject<Message>(
                    File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Message.json"));

            var activity = WebexHelper.DecryptedMessageToActivity(message);

            Assert.Equal(message.Id, activity.Id);
            Assert.Equal(ActivityTypes.Event, activity.Type);
        }
    }
}
