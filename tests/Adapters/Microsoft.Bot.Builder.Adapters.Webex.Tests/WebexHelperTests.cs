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
        public void PayloadToActivityShouldReturnNullWithNullPayload()
        {
            Assert.Null(WebexHelper.PayloadToActivity(null, _identity));
        }

        [Fact]
        public void PayloadToActivityShouldReturnActivity()
        {
            var payload = JsonConvert.DeserializeObject<WebhookEventData>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Payload.json"));

            var activity = WebexHelper.PayloadToActivity(payload, _identity);

            Assert.Equal(payload.Id, activity.Id);
            Assert.Equal(payload.ActorId, activity.From.Id);
        }

        [Fact]
        public void ValidateSignatureShouldFailWithMissingSignature()
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
        public void ValidateSignatureShouldReturnFalse()
        {
            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns("wrong_signature");
            httpRequest.Object.Body = Stream.Null;

            Assert.False(WebexHelper.ValidateSignature("test_secret", httpRequest.Object, "{}"));
        }

        [Fact]
        public async void GetDecryptedMessageAsyncShouldReturnNullWithNullPayload()
        {
            Assert.Null(await WebexHelper.GetDecryptedMessageAsync(null, null, new CancellationToken()));
        }

        [Fact]
        public async void GetDecryptedMessageAsyncShouldSucceed()
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
        public void DecryptedMessageToActivityShouldReturnNullWithNullMessage()
        {
            Assert.Null(WebexHelper.DecryptedMessageToActivity(null, _identity));
        }

        [Fact]
        public void DecryptedMessageToActivityShouldReturnActivityTypeSelfMessage()
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
        public void DecryptedMessageToActivityWithHtmlShouldReturnActivity()
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
        public void DecryptedMessageToActivityWithHtmlShouldReturnActivityWhenMatch()
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
        public void HandleMessageAttachmentsShouldFailWithMoreThanOneAttachment()
        {
            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageAttachments.json"));

            Assert.Throws<Exception>(() =>
            {
                var attachmentList = WebexHelper.HandleMessageAttachments(message);
            });
        }

        [Fact]
        public void HandleMessageAttachmentsShouldSucceed()
        {
            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Message.json"));

            var attachmentList = WebexHelper.HandleMessageAttachments(message);

            Assert.Equal(message.FileCount, attachmentList.Count);
        }

        [Fact]
        public void AttachmentActionToActivityWithNullMessageShouldFail()
        {
            Assert.Null(WebexHelper.AttachmentActionToActivity(null, _identity));
        }

        [Fact]
        public void AttachmentActionToActivityShouldReturnActivityWithEmptyText()
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
        public void AttachmentActionToActivityShouldReturnActivityWithText()
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
