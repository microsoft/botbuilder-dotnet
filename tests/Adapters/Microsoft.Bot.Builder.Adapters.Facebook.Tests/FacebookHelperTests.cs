// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Facebook.Tests
{
    public class FacebookHelperTests
    {
        public const string ExampleUrl = "http://example.com";
        public const int AttachmentCountTest = 3;

        [Fact]
        public void ActivityToFacebookShouldReturnMessageWithAttachment()
        {
            var activityJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/Activities.json");
            var activity = JsonConvert.DeserializeObject<Activity>(activityJson);

            var messageOption = FacebookHelper.ActivityToFacebook(activity);

            Assert.Equal(activity.Conversation.Id, messageOption.Recipient.Id);
            Assert.Equal(activity.Text, messageOption.Message.Text);
            Assert.Equal(activity.Attachments[0].ContentType, messageOption.Message.Attachment.Type);
        }

        [Fact]
        public void ActivityToFacebookShouldThrowNullArgumentExceptionWithActivityNull()
        {
            Assert.Throws<ArgumentNullException>(() => { FacebookHelper.ActivityToFacebook(null); });
        }

        [Fact]
        public void ProcessSingleMessageShouldThrowNullArgumentExceptionWithMessageNull()
        {
            Assert.Throws<ArgumentNullException>(() => { FacebookHelper.ProcessSingleMessage(null); });
        }

        [Fact]
        public void ProcessSingleMessageShouldReturnAnActivityWithMessageWithData()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/FacebookMessages.json");
            var facebookMessage = JsonConvert.DeserializeObject<List<FacebookMessage>>(facebookMessageJson)[0];
            var activity = FacebookHelper.ProcessSingleMessage(facebookMessage);

            Assert.Equal(activity.Conversation.Id, facebookMessage.Recipient.Id);
        }

        [Fact]
        public void ProcessSingleMessageShouldReturnConversationIdWithoutDataOnSender()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/FacebookMessages.json");
            var facebookMessage = JsonConvert.DeserializeObject<List<FacebookMessage>>(facebookMessageJson)[1];
            var activity = FacebookHelper.ProcessSingleMessage(facebookMessage);

            Assert.NotNull(activity.Conversation.Id);
            Assert.Equal(activity.Conversation.Id, facebookMessage.Recipient.Id);
        }

        [Fact]
        public void ProcessSingleMessageShouldReturnActivityWithMessage()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/FacebookMessages.json");
            var facebookMessage = JsonConvert.DeserializeObject<List<FacebookMessage>>(facebookMessageJson)[2];
            var activity = FacebookHelper.ProcessSingleMessage(facebookMessage);

            Assert.NotNull(activity.Text);
            Assert.NotNull(activity.ChannelData);
            Assert.Equal(activity.Conversation.Id, facebookMessage.Recipient.Id);
            Assert.Equal(activity.Text, facebookMessage.Message.Text);
        }

        [Fact]
        public void ProcessSingleMessageShouldReturnActivityWithAttachments()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/FacebookMessages.json");
            var facebookMessage = JsonConvert.DeserializeObject<List<FacebookMessage>>(facebookMessageJson)[3];
            var activity = FacebookHelper.ProcessSingleMessage(facebookMessage);

            Assert.NotNull(activity.Attachments);
            Assert.Equal(AttachmentCountTest, activity.Attachments.Count);
        }

        [Fact]
        public void ProcessSingleMessageShouldReturnActivityWithPostBack()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/FacebookMessages.json");
            var facebookMessage = JsonConvert.DeserializeObject<List<FacebookMessage>>(facebookMessageJson)[4];
            var activity = FacebookHelper.ProcessSingleMessage(facebookMessage);

            Assert.NotNull(activity.Text);
            Assert.Equal(facebookMessage.PostBack.Payload, activity.Text);
        }

        [Fact]
        public void ProcessSingleMessageShouldReturnActivityWithPassThreadControlRequest()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/PayloadWithPassThreadControl.json");
            var facebookResponse = JsonConvert.DeserializeObject<FacebookResponseEvent>(facebookMessageJson);

            var payload = new List<FacebookMessage>();

            payload = facebookResponse.Entry[0].Messaging;

            var activity = FacebookHelper.ProcessSingleMessage(payload[0]);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].Recipient.Id, activity.Recipient.Id);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].Sender.Id, activity.Conversation.Id);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].Sender.Id, activity.Conversation.Id);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].PassThreadControl, activity.Value);
        }

        [Fact]
        public void ProcessSingleMessageShouldReturnActivityWithRequestThreadControlRequest()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/PayloadWithRequestThreadControl.json");
            var facebookResponse = JsonConvert.DeserializeObject<FacebookResponseEvent>(facebookMessageJson);

            var payload = new List<FacebookMessage>();

            payload = facebookResponse.Entry[0].Messaging;

            var activity = FacebookHelper.ProcessSingleMessage(payload[0]);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].Recipient.Id, activity.Recipient.Id);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].Sender.Id, activity.Conversation.Id);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].Sender.Id, activity.Conversation.Id);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].RequestThreadControl, activity.Value);
        }

        [Fact]
        public void ProcessSingleMessageShouldReturnActivityWithTakeThreadControlRequest()
        {
            var facebookMessageJson = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/PayloadWithTakeThreadControl.json");
            var facebookResponse = JsonConvert.DeserializeObject<FacebookResponseEvent>(facebookMessageJson);

            var payload = new List<FacebookMessage>();

            payload = facebookResponse.Entry[0].Messaging;

            var activity = FacebookHelper.ProcessSingleMessage(payload[0]);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].Recipient.Id, activity.Recipient.Id);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].Sender.Id, activity.Conversation.Id);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].Sender.Id, activity.Conversation.Id);
            Assert.Equal(facebookResponse.Entry[0].Messaging[0].TakeThreadControl, activity.Value);
        }

        [Fact]
        public async Task WriteAsyncShouldFailWithNullResponse()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await FacebookHelper.WriteAsync(null, HttpStatusCode.OK, "testText", Encoding.UTF8, new CancellationToken()).ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task WriteAsyncShouldFailWithNullText()
        {
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await FacebookHelper.WriteAsync(httpResponse.Object, HttpStatusCode.OK, null, Encoding.UTF8, new CancellationToken()).ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task WriteAsyncShouldFailWithNullEncoding()
        {
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await FacebookHelper.WriteAsync(httpResponse.Object, HttpStatusCode.OK, "testText", null, new CancellationToken()).ConfigureAwait(false);
            });
        }
    }
}
