// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Thrzn41.WebexTeams.Version1;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Webex.Tests
{
    public class WebexHelperTests
    {
        private readonly Person _identity = JsonConvert.DeserializeObject<Person>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Person.json"));

        [Fact]
        public void PayloadToActivity_Should_Return_Null_With_Null_Payload()
        {
            Assert.Null(WebexHelper.PayloadToActivity(null, _identity));
        }

        [Fact]
        public void PayloadToActivity_Should_Return_Activity()
        {
            var payload = JsonConvert.DeserializeObject<WebhookEventData>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Payload.json"));

            var activity = WebexHelper.PayloadToActivity(payload, _identity);

            Assert.Equal(payload.Id, activity.Id);
            Assert.Equal(payload.ActorId, activity.From.Id);
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
        public async void GetDecryptedMessageAsync_Should_Return_Null_With_Null_Payload()
        {
            Assert.Null(await WebexHelper.GetDecryptedMessageAsync(null, null, new CancellationToken()));
        }

        [Fact]
        public async void GetDecryptedMessageAsync_Should_Succeed()
        {
            var payload = JsonConvert.DeserializeObject<WebhookEventData>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Payload.json"));

            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Message.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(message));

            var actualMessage = await WebexHelper.GetDecryptedMessageAsync(payload, webexApi.Object.GetMessageAsync, new CancellationToken());

            Assert.Equal(message.Id, actualMessage.Id);
        }

        [Fact]
        public void DecryptedMessageToActivity_Should_Return_Null_With_Null_Message()
        {
            Assert.Null(WebexHelper.DecryptedMessageToActivity(null, _identity));
        }

        [Fact]
        public void DecryptedMessageToActivity_Should_Return_Activity_Type_SelfMessage()
        {
            var serializedPerson = "{\"id\":\"person_id\"}";
            var identity = JsonConvert.DeserializeObject<Person>(serializedPerson);

            var message =
                JsonConvert.DeserializeObject<Message>(
                    File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Message.json"));

            var activity = WebexHelper.DecryptedMessageToActivity(message, identity);

            Assert.Equal(message.Id, activity.Id);
            Assert.Equal(ActivityTypes.Event, activity.Type);
        }

        [Fact]
        public void DecryptedMessageToActivity_With_Html_Should_Return_Activity()
        {
            var serializedPerson = "{\"id\":\"different_id\"}";
            var identity = JsonConvert.DeserializeObject<Person>(serializedPerson);

            var message =
                JsonConvert.DeserializeObject<Message>(
                    File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageHtml.json"));

            var activity = WebexHelper.DecryptedMessageToActivity(message, identity);

            Assert.Equal(message.Id, activity.Id);
            Assert.Equal(message.Html, activity.Text);
        }

        [Fact]
        public void DecryptedMessageToActivity_With_Html_Should_Return_Activity_When_Match()
        {
            var serializedPerson = "{\"id\":\"/ciscospark://us/PEOPLE/different_id\"}";
            var identity = JsonConvert.DeserializeObject<Person>(serializedPerson);

            var message =
                JsonConvert.DeserializeObject<Message>(
                    File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageHtml.json"));

            var activity = WebexHelper.DecryptedMessageToActivity(message, identity);

            Assert.Equal(message.Id, activity.Id);
            Assert.Equal(message.Html, activity.Text);
        }

        [Fact]
        public void HandleMessageAttachments_Should_Fail_With_MoreThanOne_Attachment()
        {
            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageAttachments.json"));

            Assert.Throws<Exception>(() =>
            {
                var attachmentList = WebexHelper.HandleMessageAttachments(message);
            });
        }

        [Fact]
        public void HandleMessageAttachments_Should_Succeed()
        {
            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Message.json"));

            var attachmentList = WebexHelper.HandleMessageAttachments(message);

            Assert.Equal(message.FileCount, attachmentList.Count);
        }

        [Fact]
        public void AttachmentActionToActivity_With_Null_Message_Should_Fail()
        {
            Assert.Null(WebexHelper.AttachmentActionToActivity(null, _identity));
        }

        [Fact]
        public void AttachmentActionToActivity_Should_Return_Activity_With_Empty_Text()
        {
            var message =
                JsonConvert.DeserializeObject<Message>(
                    File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageWithInputs.json"));

            var data = JsonConvert.SerializeObject(message);
            var messageExtraData = JsonConvert.DeserializeObject<AttachmentActionData>(data);

            var activity = WebexHelper.AttachmentActionToActivity(message, _identity);

            Assert.Equal(message.Id, activity.Id);
            Assert.Equal(messageExtraData.Inputs, activity.Value);
            Assert.Equal(string.Empty, activity.Text);
        }

        [Fact]
        public void AttachmentActionToActivity_Should_Return_Activity_With_Text()
        {
            var message =
                JsonConvert.DeserializeObject<Message>(
                    File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageWithText.json"));

            var data = JsonConvert.SerializeObject(message);
            var messageExtraData = JsonConvert.DeserializeObject<AttachmentActionData>(data);

            var activity = WebexHelper.AttachmentActionToActivity(message, _identity);

            Assert.Equal(message.Id, activity.Id);
            Assert.Equal(messageExtraData.Inputs, activity.Value);
            Assert.Equal(message.Text, activity.Text);
        }
    }
}
