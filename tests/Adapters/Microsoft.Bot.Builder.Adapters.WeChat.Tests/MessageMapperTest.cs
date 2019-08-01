using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event;
using Microsoft.Bot.Builder.Adapters.WeChat.Tests.TestUtilities;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Tests
{
    public class MessageMapperTest
    {
        private readonly WeChatMessageMapper wechatMessageMapper;
        private readonly WeChatMessageMapper wechatMessageMapper2;

        public MessageMapperTest()
        {
            var wechatClient = MockDataUtility.GetMockWeChatClient();
            this.wechatMessageMapper = new WeChatMessageMapper(wechatClient, true);
            this.wechatMessageMapper2 = new WeChatMessageMapper(wechatClient, false);
        }

        [Fact]
        public async Task ToConnectorMessageTest_TestRequest()
        {
            var mockRequestList = MockDataUtility.GetMockRequestMessageList();
            foreach (var mockRequest in mockRequestList)
            {
                var activity = await wechatMessageMapper.ToConnectorMessage(mockRequest);
                AssertGeneralParameters(mockRequest, activity);
            }
        }

        [Fact]
        public async Task ToWeChatMessagesTest_MessageActivity()
        {
            var activityList = MockDataUtility.GetMockMessageActivityList();
            var secretInfo = MockDataUtility.GetMockSecretInfo();
            foreach (var messageActivity in activityList)
            {
                var wechatResponses = await wechatMessageMapper.ToWeChatMessages(messageActivity, secretInfo);
                var wechatResponses2 = await wechatMessageMapper2.ToWeChatMessages(messageActivity, secretInfo);
                Assert.True(wechatResponses.Count > 0);
                Assert.True(wechatResponses2.Count > 0);
            }
        }

        [Fact]
        public async Task ToWeChatMessagesTest_MessageActivityWithAttachment()
        {
            var messageActivity = MockDataUtility.GetMockMessageActivity();
            var attachments = await MockDataUtility.GetGeneralAttachmentList();
            foreach (var att in attachments)
            {
                messageActivity.Attachments.Add(att);
            }

            var secretInfo = MockDataUtility.GetMockSecretInfo();
            var wechatResponses = await wechatMessageMapper.ToWeChatMessages(messageActivity, secretInfo);
            var wechatResponses2 = await wechatMessageMapper2.ToWeChatMessages(messageActivity, secretInfo);
            Assert.True(wechatResponses2.Count > 0);
            Assert.True(wechatResponses.Count > 0);
        }

        [Fact]
        public async Task ToWeChatMessagesTest_EventActivity()
        {
            var activityList = MockDataUtility.GetMockEventActivityList();
            var secretInfo = MockDataUtility.GetMockSecretInfo();
            foreach (var activity in activityList)
            {
                var wechatResponses = await wechatMessageMapper.ToWeChatMessages(activity, secretInfo);

                // Assert.True(wechatResponses.Count > 0);
            }
        }

        [Fact]
        public void MapperUtilsTest()
        {
            var testString = "test";
            Assert.Equal(testString + "\r\n" + testString, testString.AddLine(testString));
            Assert.Equal(testString + "  " + testString, testString.AddText(testString));
            Assert.Equal(".png", MapperUtils.GetMediaExtension("http://test.jpg", "image/png", UploadMediaType.Image));
            Assert.Equal(".jpg", MapperUtils.GetMediaExtension("http://test.jpg", "image", UploadMediaType.Image));
            Assert.Equal(".mp3", MapperUtils.GetMediaExtension("http://test.mp3", "audio", UploadMediaType.Voice));
            Assert.Equal(".amr", MapperUtils.GetMediaExtension("http://test.mp3", "audio/amr", UploadMediaType.Voice));
            Assert.Equal(".mp4", MapperUtils.GetMediaExtension("http://test.mp4", "video", UploadMediaType.Video));
            Assert.Equal(".ogv", MapperUtils.GetMediaExtension("http://test.jpg", "video/ogg", UploadMediaType.Video));
            Assert.Equal(".png", MapperUtils.GetMediaExtension("http://test.jpg", "image/png", UploadMediaType.Thumb));
            Assert.Equal(".jpg", MapperUtils.GetMediaExtension("http://test", "bin", UploadMediaType.Thumb));
            Assert.Equal(".mp4", MapperUtils.GetMediaExtension("http://test", "bin", UploadMediaType.Video));
            Assert.Equal(".mp3", MapperUtils.GetMediaExtension("http://test", "bin", UploadMediaType.Voice));
        }

        private void AssertGeneralParameters(IRequestMessageBase requestMessage, IActivity activity)
        {
            Assert.Equal(requestMessage.ToUserName, activity.Recipient.Id);
            Assert.Equal("Bot", activity.Recipient.Name);
            Assert.Equal(requestMessage.FromUserName, activity.From.Id);
            Assert.Equal("User", activity.From.Name);
            var test = requestMessage as RequestMessage;
            if (requestMessage is RequestMessage message)
            {
                Assert.Equal(message.MsgId.ToString(), activity.Id);
            }
            else
            {
                Assert.True(requestMessage is IRequestMessageEventBase);
            }

            Assert.Equal("wechat", activity.ChannelId);
            Assert.Equal(requestMessage.FromUserName, activity.Conversation.Id);
        }
    }
}
