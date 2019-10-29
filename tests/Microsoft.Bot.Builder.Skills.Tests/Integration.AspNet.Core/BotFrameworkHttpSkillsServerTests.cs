// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Skills.Tests.Integration.AspNet.Core
{
    public class BotFrameworkHttpSkillsServerTests
    {
        [Theory]
        [InlineData(ChannelApiMethods.GetActivityMembers, "GET", "/v3/conversations/SomeConversationId/activities/SomeActivityId/members")]
        [InlineData(ChannelApiMethods.GetActivityMembers, "GET", "/api/v1/messages/bots/somOtherStuff/v3/conversations/SomeConversationId/activities/SomeActivityId/members")]
        [InlineData(ChannelApiMethods.ReplyToActivity, "POST", "/v3/conversations/SomeConversationId/activities/SomeActivityId")]
        [InlineData(ChannelApiMethods.ReplyToActivity, "POST", "/api/v1/messages/bots/somOtherStuff/v3/conversations/SomeConversationId/activities/SomeActivityId")]
        [InlineData(ChannelApiMethods.UpdateActivity, "PUT", "/v3/conversations/conversationId/activities/activityId")]
        [InlineData(ChannelApiMethods.UpdateActivity, "PUT", "/api/v1/messages/bots/somOtherStuff/v3/conversations/conversationId/activities/activityId")]
        [InlineData(ChannelApiMethods.DeleteActivity, "DELETE", "/v3/conversations/conversationId/activities/activityId")]
        [InlineData(ChannelApiMethods.DeleteActivity, "DELETE", "/api/v1/messages/bots/somOtherStuff/v3/conversations/conversationId/activities/activityId")]
        [InlineData(ChannelApiMethods.SendToConversation, "POST", "/v3/conversations/conversationId/activities")]
        [InlineData(ChannelApiMethods.SendToConversation, "POST", "/api/v1/messages/bots/somOtherStuff/v3/conversations/conversationId/activities")]
        [InlineData(ChannelApiMethods.SendConversationHistory, "POST", "/v3/conversations/conversationId/activities/history")]
        [InlineData(ChannelApiMethods.SendConversationHistory, "POST", "/api/v1/messages/bots/somOtherStuff/v3/conversations/conversationId/activities/history")]
        [InlineData(ChannelApiMethods.DeleteConversationMember, "DELETE", "/v3/conversations/conversationId/members/memberId")]
        [InlineData(ChannelApiMethods.DeleteConversationMember, "DELETE", "/api/v1/messages/bots/somOtherStuff/v3/conversations/conversationId/members/memberId")]
        [InlineData(ChannelApiMethods.UploadAttachment, "POST", "/v3/conversations/conversationId/attachments")]
        [InlineData(ChannelApiMethods.UploadAttachment, "POST", "/api/v1/messages/bots/somOtherStuff/v3/conversations/conversationId/attachments")]
        [InlineData(ChannelApiMethods.GetConversationMembers, "GET", "/v3/conversations/conversationId/members")]
        [InlineData(ChannelApiMethods.GetConversationMembers, "GET", "/api/v1/messages/bots/somOtherStuff/v3/conversations/conversationId/members")]
        [InlineData(ChannelApiMethods.GetConversationPagedMembers, "GET", "/v3/conversations/conversationId/pagedmember")]
        [InlineData(ChannelApiMethods.GetConversationPagedMembers, "GET", "/api/v1/messages/bots/somOtherStuff/v3/conversations/conversationId/pagedmember")]
        [InlineData(ChannelApiMethods.GetConversations, "GET", "/v3/conversations/")]
        [InlineData(ChannelApiMethods.GetConversations, "GET", "/api/v1/messages/bots/somOtherStuff/v3/conversations/")]
        [InlineData(ChannelApiMethods.CreateConversation, "POST", "/v3/conversations/")]
        [InlineData(ChannelApiMethods.CreateConversation, "POST", "/api/v1/messages/bots/somOtherStuff/v3/conversations/")]
        public void GetActionForPath(string expectedMethod, string httpMethod, string path)
        {
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Method).Returns(httpMethod);
            mockRequest.Setup(x => x.Path).Returns(path);

            var routeAction = BotFrameworkHttpSkillsServer.GetRoute(mockRequest.Object);

            Assert.Equal(expectedMethod, routeAction.Method);
        }
    }
}
