// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events;
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
            foreach (var messageActivity in activityList)
            {
                var wechatResponses = await wechatMessageMapper.ToWeChatMessages(messageActivity);
                var wechatResponses2 = await wechatMessageMapper2.ToWeChatMessages(messageActivity);
                Assert.True(wechatResponses.Count > 0);
                Assert.True(wechatResponses2.Count > 0);
            }
        }

        [Fact]
        public async Task ToWeChatMessagesTest_MessageActivityWithAttachment()
        {
            var messageActivity = MockDataUtility.GetMockMessageActivity();
            var attachments = await MockDataUtility.GetGeneralAttachmentList(true);
            foreach (var att in attachments)
            {
                messageActivity.Attachments.Add(att);
            }

            var wechatResponses = await wechatMessageMapper.ToWeChatMessages(messageActivity);
            var wechatResponses2 = await wechatMessageMapper2.ToWeChatMessages(messageActivity);
            Assert.True(wechatResponses2.Count > 0);
            Assert.True(wechatResponses.Count > 0);
        }

        [Fact]
        public async Task ToWeChatMessagesTest_EventActivity()
        {
            var activityList = MockDataUtility.GetMockEventActivityList();
            foreach (var activity in activityList)
            {
                var wechatResponses = await wechatMessageMapper.ToWeChatMessages(activity);
            }
        }

        private void AssertGeneralParameters(IRequestMessageBase requestMessage, IActivity activity)
        {
            Assert.Equal(requestMessage.ToUserName, activity.Recipient.Id);
            Assert.Equal("Bot", activity.Recipient.Name);
            Assert.Equal(requestMessage.FromUserName, activity.From.Id);
            Assert.Equal("User", activity.From.Name);
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
