// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Xunit;

namespace Microsoft.Bot.Connector.Tests
{
    public class AttachmentsTests : BaseTest
    {
        protected const string ConversationId = "B21UTEF8S:T03CWQ0QB:D2369CT7C";

        [Fact]
        public async Task UploadAttachmentAndGetAttachment()
        {
            await UseClientFor(async client =>
            {
                var attachment = new AttachmentData("image/png", "Bot.png", ReadFile("bot.png"), ReadFile("bot_icon.png"));
                var response = await client.Conversations.UploadAttachmentAsync(ConversationId, attachment);
                var attachmentId = response.Id;
                var attachmentInfo = await client.Attachments.GetAttachmentInfoAsync(attachmentId);

                Assert.NotNull(attachmentInfo);
                Assert.Equal("Bot.png", attachmentInfo.Name);
                Assert.Equal("image/png", attachmentInfo.Type);
                Assert.Equal(2, attachmentInfo.Views.Count);
            });
        }

        [Fact]
        public async Task UploadAttachmentAndGetAttachment_WithTracing()
            => await AssertTracingFor(UploadAttachmentAndGetAttachment, nameof(ConversationsExtensions.UploadAttachmentAsync));

        [Fact]
        public async Task UploadAttachmentWithoutOriginalFails()
        {
            await UseClientFor(async client =>
            {
                var attachment = new AttachmentData()
                {
                    Name = "Bot.png",
                    Type = "image/png",
                };

                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Conversations.UploadAttachmentAsync(ConversationId, attachment));
                Assert.Equal("MissingProperty", ex.Body.Error.Code);
                Assert.Contains("original", ex.Body.Error.Message);
            });
        }

        [Fact]
        public async Task UploadAttachmentWithoutOriginalFails_WithTracing()
            => await AssertTracingFor(UploadAttachmentWithoutOriginalFails, nameof(ConversationsExtensions.UploadAttachmentAsync), isSuccesful: false);

        [Fact]
        public async Task UploadAttachmentWithNullConversationId()
        {
            await UseClientFor(async client =>
            {
                var attachment = new AttachmentData("image/png", "Bot.png", ReadFile("bot.png"), ReadFile("bot_icon.png"));

                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.UploadAttachmentAsync(null, attachment));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task UploadAttachmentWithNullAttachment()
        {
            await UseClientFor(async client =>
            {
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.UploadAttachmentAsync(ConversationId, null));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task UploadAttachmentAndGetAttachmentWithCustomHeader()
        {
            var customHeaders = new Dictionary<string, List<string>>() { { "customHeader", new List<string>() { "customValue" } } };

            await AssertTracingFor(
                async () =>
                await UseClientFor(async client =>
                {
                    var attachment = new AttachmentData("image/png", "Bot.png", ReadFile("bot.png"), ReadFile("bot_icon.png"));
                    var response = await client.Conversations.UploadAttachmentWithHttpMessagesAsync(ConversationId, attachment, customHeaders);
                    Assert.NotNull(response.Body);
                    Assert.NotNull(response.Body.Id);
                }),
                nameof(ConversationsExtensions.UploadAttachmentAsync),
                assertHttpRequestMessage:
                    (h) => h.Headers.Contains("customHeader") && h.Headers.GetValues("customHeader").Contains("customValue"));
        }

        [Fact]
        public async Task GetAttachmentInfoWithInvalidIdFails()
        {
            await UseClientFor(async client =>
            {
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Attachments.GetAttachmentInfoAsync("bt13796-GJS4yaxDLI"));
                Assert.Contains("NotFound", ex.Message);
            });
        }

        [Fact]
        public async Task GetAttachmentInfoWithInvalidIdFails_WithTracing()
            => await AssertTracingFor(GetAttachmentInfoWithInvalidIdFails, nameof(AttachmentsExtensions.GetAttachmentInfoAsync), isSuccesful: false);

        [Fact]
        public async Task GetAttachmentInfoWithNullIdFails()
        {
            await UseClientFor(async client =>
            {
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Attachments.GetAttachmentInfoAsync(null));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task GetAttachmentView()
        {
            await UseClientFor(async client =>
            {
                var attachment = new AttachmentData("image/png", "Bot.png", ReadFile("bot.png"), ReadFile("bot_icon.png"));
                var response = await client.Conversations.UploadAttachmentAsync(ConversationId, attachment);
                var attachmentId = response.Id;
                var stream = await client.Attachments.GetAttachmentAsync(attachmentId, "original");

                // Workaround for TestFramework not saving/replaying binary content
                // Instead, convert the expected output the same way that the TestRecorder converts binary content to string
                var expectedAsString = new StreamReader(new MemoryStream(attachment.OriginalBase64)).ReadToEnd();
                var actualAsString = new StreamReader(stream).ReadToEnd();

                Assert.Equal(expectedAsString, actualAsString);
            });
        }

        [Fact]
        public async Task GetAttachmentView_WithTracing()
            => await AssertTracingFor(GetAttachmentView, nameof(AttachmentsExtensions.GetAttachmentAsync));

        [Fact]
        public async Task GetAttachmentViewWithInvalidAttachmentIdFails()
        {
            await UseClientFor(async client =>
            {
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Attachments.GetAttachmentAsync("bt13796-GJS4yaxDLI", "original"));
                Assert.Contains("NotFound", ex.Message);
            });
        }

        [Fact]
        public async Task GetAttachmentViewWithInvalidAttachmentIdFails_WithTracing()
            => await AssertTracingFor(GetAttachmentViewWithInvalidAttachmentIdFails, nameof(AttachmentsExtensions.GetAttachmentAsync), isSuccesful: false);

        [Fact]
        public async Task GetAttachmentViewWithNullAttachmentIdFails()
        {
            await UseClientFor(async client =>
            {
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Attachments.GetAttachmentAsync(null, "original"));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task GetAttachmentViewWithInvalidViewIdFails()
        {
            await UseClientFor(async client =>
            {
                var attachment = new AttachmentData("image/png", "Bot.png", ReadFile("bot.png"), ReadFile("bot_icon.png"));
                var response = await client.Conversations.UploadAttachmentAsync(ConversationId, attachment);

                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Attachments.GetAttachmentAsync(response.Id, "invalid"));

                Assert.Contains("NotFound", ex.Message);
            });
        }

        [Fact]
        public async Task GetAttachmentViewWithInvalidViewIdFails_WithTracing()
            => await AssertTracingFor(GetAttachmentViewWithInvalidViewIdFails, nameof(AttachmentsExtensions.GetAttachmentAsync), isSuccesful: false);

        [Fact]
        public async Task GetAttachmentViewWithNullViewIdFails()
        {
            await UseClientFor(async client =>
            {
                var attachment = new AttachmentData("image/png", "Bot.png", ReadFile("bot.png"), ReadFile("bot_icon.png"));
                var response = await client.Conversations.UploadAttachmentAsync(ConversationId, attachment);

                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Attachments.GetAttachmentAsync(response.Id, null));

                Assert.Contains("cannot be null", ex.Message);
            });
        }

        private static byte[] ReadFile(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "resources", fileName);
            return File.ReadAllBytes(path);
        }
    }
}
