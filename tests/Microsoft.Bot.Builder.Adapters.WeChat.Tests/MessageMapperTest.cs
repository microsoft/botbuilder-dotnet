using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event;
using Microsoft.Bot.Builder.Adapters.WeChat.Test.TestUtilities;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Test
{
    [TestClass]
    public class MessageMapperTest
    {
        private WeChatMessageMapper wechatMessageMapper;

        public MessageMapperTest()
        {
            var wechatClient = MockDataUtility.MockWeChatClient();
            var configuration = MockDataUtility.MockConfiguration();
            this.wechatMessageMapper = new WeChatMessageMapper(configuration, wechatClient);
        }

        [TestMethod]
        public async Task ToConnectorMessageTest_TestRequest()
        {
            var mockRequestList = MockDataUtility.GetMockRequestMessageList();
            foreach (var mockRequest in mockRequestList)
            {
                var activity = await wechatMessageMapper.ToConnectorMessage(mockRequest);
                AssertGeneralParameters(mockRequest, activity);
            }
        }

        [TestMethod]
        public async Task ToWeChatMessagesTest_MessageActivity()
        {
            var activityList = MockDataUtility.GetMockMessageActivityList();
            var secretInfo = MockDataUtility.GetMockSecretInfo();
            foreach (var messageActivity in activityList)
            {
                var wechatResponses = await wechatMessageMapper.ToWeChatMessages(messageActivity, secretInfo);
                Assert.IsTrue(wechatResponses.Count > 0);
            }
        }

        [TestMethod]
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
            Assert.IsTrue(wechatResponses.Count > 0);
        }

        [TestMethod]
        public async Task ToWeChatMessagesTest_EventActivity()
        {
            var activityList = MockDataUtility.GetMockEventActivityList();
            var secretInfo = MockDataUtility.GetMockSecretInfo();
            foreach (var activity in activityList)
            {
                var wechatResponses = await wechatMessageMapper.ToWeChatMessages(activity, secretInfo);

                // Assert.IsTrue(wechatResponses.Count > 0);
            }
        }

        [TestMethod]
        public void MapperUtilsTest()
        {
            var testString = "test";
            Assert.AreEqual(testString + "\n\n" + testString, testString.AddLine(testString));
            Assert.AreEqual(testString + "  " + testString, testString.AddText(testString));
            Assert.AreEqual(".png", MapperUtils.GetMediaExtension("http://test.jpg", "image/png", UploadMediaType.Image));
            Assert.AreEqual(".jpg", MapperUtils.GetMediaExtension("http://test.jpg", "image", UploadMediaType.Image));
            Assert.AreEqual(".mp3", MapperUtils.GetMediaExtension("http://test.mp3", "audio", UploadMediaType.Voice));
            Assert.AreEqual(".amr", MapperUtils.GetMediaExtension("http://test.mp3", "audio/amr", UploadMediaType.Voice));
            Assert.AreEqual(".mp4", MapperUtils.GetMediaExtension("http://test.mp4", "video", UploadMediaType.Video));
            Assert.AreEqual(".ogv", MapperUtils.GetMediaExtension("http://test.jpg", "video/ogg", UploadMediaType.Video));
            Assert.AreEqual(".png", MapperUtils.GetMediaExtension("http://test.jpg", "image/png", UploadMediaType.Thumb));
            Assert.AreEqual(".jpg", MapperUtils.GetMediaExtension("http://test", "image", UploadMediaType.Thumb));
        }

        private void AssertGeneralParameters(IRequestMessageBase requestMessage, IActivity activity)
        {
            Assert.AreEqual(requestMessage.ToUserName, activity.Recipient.Id);
            Assert.AreEqual("Bot", activity.Recipient.Name);
            Assert.AreEqual(requestMessage.FromUserName, activity.From.Id);
            Assert.AreEqual("User", activity.From.Name);
            var test = requestMessage as RequestMessage;
            if (requestMessage is RequestMessage message)
            {
                Assert.AreEqual(message.MsgId.ToString(), activity.Id);
            }
            else
            {
                Assert.IsTrue(requestMessage is IRequestMessageEventBase);
            }

            Assert.AreEqual(Constants.ChannelId, activity.ChannelId);
            Assert.AreEqual(requestMessage.FromUserName, activity.Conversation.Id);
        }
    }
}
