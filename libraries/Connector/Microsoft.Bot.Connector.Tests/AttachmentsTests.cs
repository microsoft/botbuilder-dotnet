//namespace Connector.Tests
//{
//    using System;
//    using System.IO;
//    using Microsoft.Bot.Connector;
    
//    using Microsoft.Rest;
//    using Xunit;

//    public class AttachmentsTests : BaseTest
//    {
//        protected const string conversationId = "B21UTEF8S:T03CWQ0QB:D2369CT7C";

//        [Fact]
//        public void UploadAttachmentAndGetAttachment()
//        {
//            UseClientFor(async client =>
//            {
//                var attachment = new AttachmentData("image/png", ReadFile("bot.png"), "Bot.png", ReadFile("bot_icon.png"));
//                var response = await client.Conversations.UploadAttachmentAsync(conversationId, attachment);
//                var attachmentId = response.Id;
//                var attachmentInfo = await client.Attachments.GetAttachmentInfoAsync(attachmentId);

//                Assert.NotNull(attachmentInfo);
//                Assert.Equal("Bot.png", attachmentInfo.Name);
//                Assert.Equal("image/png", attachmentInfo.Type);
//                Assert.Equal(2, attachmentInfo.Views.Count);
//            });
//        }

//        [Fact]
//        public void UploadAttachmentWithoutOriginalFails()
//        {
//            UseClientFor(async client =>
//            {
//                var attachment = new AttachmentData()
//                {
//                    Name = "Bot.png",
//                    Type = "image/png"
//                };

//                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.UploadAttachmentAsync(conversationId, attachment));
//                Assert.Contains("OriginalBase64", ex.Message);
//            });
//        }

//        [Fact]
//        public void UploadAttachmentWithoutTypeFails()
//        {
//            UseClientFor(async client =>
//            {
//                var attachment = new AttachmentData()
//                {
//                    Name = "Bot.png",
//                    OriginalBase64 = ReadFile("Bot.png")
//                };

//                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.UploadAttachmentAsync(conversationId, attachment));
//                Assert.Contains("Type", ex.Message);
//            });
//        }

//        [Fact]
//        public void UploadAttachmentWithNullConversationId()
//        {
//            UseClientFor(async client =>
//            {
//                var attachment = new AttachmentData("image/png", ReadFile("bot.png"), "Bot.png", ReadFile("bot_icon.png"));

//                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.UploadAttachmentAsync(null, attachment));
//                Assert.Contains("cannot be null", ex.Message);
//            });
//        }

//        [Fact]
//        public void UploadAttachmentWithNullAttachment()
//        {
//            UseClientFor(async client =>
//            {
//                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.UploadAttachmentAsync(conversationId, null));
//                Assert.Contains("cannot be null", ex.Message);
//            });
//        }

//        [Fact]
//        public void GetAttachmentInfoWithInvalidIdFails()
//        {
//            UseClientFor(async client =>
//            {
//                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Attachments.GetAttachmentInfoAsync("bt13796-GJS4yaxDLI"));
//                Assert.Contains("NotFound", ex.Message);
//            });
//        }

//        [Fact]
//        public void GetAttachmentInfoWithNullIdFails()
//        {
//            UseClientFor(async client =>
//            {
//                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Attachments.GetAttachmentInfoAsync(null));
//                Assert.Contains("cannot be null", ex.Message);
//            });
//        }

//        [Fact]
//        public void GetAttachmentView()
//        {
//            UseClientFor(async client =>
//            {
//                var attachment = new AttachmentData("image/png", ReadFile("bot.png"), "Bot.png", ReadFile("bot_icon.png"));
//                var response = await client.Conversations.UploadAttachmentAsync(conversationId, attachment);
//                var attachmentId = response.Id;
//                var stream = await client.Attachments.GetAttachmentAsync(attachmentId, "original");

//                // Workaround for TestFramework not saving/replaying binary content
//                // Instead, convert the expected output the same way that the TestRecorder converts binary content to string
//                var expectedAsString = new StreamReader(new MemoryStream(attachment.OriginalBase64)).ReadToEnd();
//                var actualAsString = new StreamReader(stream).ReadToEnd();

//                Assert.Equal(expectedAsString, actualAsString);
//            });
//        }

//        [Fact]
//        public void GetAttachmentViewWithInvalidAttachmentIdFails()
//        {
//            UseClientFor(async client =>
//            {
//                var ex = await Assert.ThrowsAsync<HttpOperationException>(() => client.Attachments.GetAttachmentAsync("bt13796-GJS4yaxDLI", "original"));
//                Assert.Contains("NotFound", ex.Message);
//            });
//        }

//        [Fact]
//        public void GetAttachmentViewWithNullAttachmentIdFails()
//        {

//            UseClientFor(async client =>
//            {
//                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Attachments.GetAttachmentAsync(null, "original"));
//                Assert.Contains("cannot be null", ex.Message);
//            });
//        }

//        [Fact]
//        public void GetAttachmentViewWithInvalidViewIdFails()
//        {

//            UseClientFor(async client =>
//            {
//                var attachment = new AttachmentData("image/png", ReadFile("bot.png"), "Bot.png", ReadFile("bot_icon.png"));
//                var response = await client.Conversations.UploadAttachmentAsync(conversationId, attachment);

//                var ex = await Assert.ThrowsAsync<HttpOperationException>(() => client.Attachments.GetAttachmentAsync(response.Id, "invalid"));

//                Assert.Contains("NotFound", ex.Message);
//            });
//        }

//        [Fact]
//        public void GetAttachmentViewWithNullViewIdFails()
//        {

//            UseClientFor(async client =>
//            {
//                var attachment = new AttachmentData("image/png", ReadFile("bot.png"), "Bot.png", ReadFile("bot_icon.png"));
//                var response = await client.Conversations.UploadAttachmentAsync(conversationId, attachment);

//                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Attachments.GetAttachmentAsync(response.Id, null));

//                Assert.Contains("cannot be null", ex.Message);
//            });
//        }

//        private byte[] ReadFile(string fileName)
//        {
//            var path = Path.Combine(Directory.GetCurrentDirectory(), "resources", fileName);
//            return File.ReadAllBytes(path);
//        }
//    }
//}
