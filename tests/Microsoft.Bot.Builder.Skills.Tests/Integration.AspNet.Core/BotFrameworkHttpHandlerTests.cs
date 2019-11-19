// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Skills.Tests.Integration.AspNet.Core
{
    public class BotFrameworkHttpHandlerTests
    {
        [Theory]
        [InlineData(ChannelApiMethods.GetActivityMembers, "GET", "/v3/conversations/SomeConversationId/activities/SomeActivityId/members")]
        [InlineData(ChannelApiMethods.GetActivityMembers, "GET", "/someVDir/v3/conversations/SomeConversationId/activities/SomeActivityId/members")]
        [InlineData(ChannelApiMethods.ReplyToActivity, "POST", "/v3/conversations/SomeConversationId/activities/SomeActivityId")]
        [InlineData(ChannelApiMethods.ReplyToActivity, "POST", "/someVDir/v3/conversations/SomeConversationId/activities/SomeActivityId")]
        [InlineData(ChannelApiMethods.UpdateActivity, "PUT", "/v3/conversations/conversationId/activities/activityId")]
        [InlineData(ChannelApiMethods.UpdateActivity, "PUT", "/someVDir/v3/conversations/conversationId/activities/activityId")]
        [InlineData(ChannelApiMethods.DeleteActivity, "DELETE", "/v3/conversations/conversationId/activities/activityId")]
        [InlineData(ChannelApiMethods.DeleteActivity, "DELETE", "/someVDir/v3/conversations/conversationId/activities/activityId")]
        [InlineData(ChannelApiMethods.SendToConversation, "POST", "/v3/conversations/conversationId/activities")]
        [InlineData(ChannelApiMethods.SendToConversation, "POST", "/someVDir/v3/conversations/conversationId/activities")]
        [InlineData(ChannelApiMethods.SendConversationHistory, "POST", "/v3/conversations/conversationId/activities/history")]
        [InlineData(ChannelApiMethods.SendConversationHistory, "POST", "/someVDir/v3/conversations/conversationId/activities/history")]
        [InlineData(ChannelApiMethods.DeleteConversationMember, "DELETE", "/v3/conversations/conversationId/members/memberId")]
        [InlineData(ChannelApiMethods.DeleteConversationMember, "DELETE", "/someVDir/v3/conversations/conversationId/members/memberId")]
        [InlineData(ChannelApiMethods.UploadAttachment, "POST", "/v3/conversations/conversationId/attachments")]
        [InlineData(ChannelApiMethods.UploadAttachment, "POST", "/someVDir/v3/conversations/conversationId/attachments")]
        [InlineData(ChannelApiMethods.GetConversationMembers, "GET", "/v3/conversations/conversationId/members")]
        [InlineData(ChannelApiMethods.GetConversationMembers, "GET", "/someVDir/v3/conversations/conversationId/members")]
        [InlineData(ChannelApiMethods.GetConversationPagedMembers, "GET", "/v3/conversations/conversationId/pagedmember")]
        [InlineData(ChannelApiMethods.GetConversationPagedMembers, "GET", "/someVDir/v3/conversations/conversationId/pagedmember")]
        [InlineData(ChannelApiMethods.GetConversations, "GET", "/v3/conversations/")]
        [InlineData(ChannelApiMethods.GetConversations, "GET", "/someVDir/v3/conversations/")]
        [InlineData(ChannelApiMethods.CreateConversation, "POST", "/v3/conversations/")]
        [InlineData(ChannelApiMethods.CreateConversation, "POST", "/someVDir/v3/conversations/")]
        public void GetActionForPath(string expectedMethod, string httpMethod, string path)
        {
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Method).Returns(httpMethod);
            mockRequest.Setup(x => x.Path).Returns(path);

            var routeAction = BotFrameworkHttpHandler.GetRoute(mockRequest.Object);

            Assert.Equal(expectedMethod, routeAction.Method);
        }

        [Fact]
        public void GetActionForPathReturnsNull()
        {
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Method).Returns("POST");
            mockRequest.Setup(x => x.Path).Returns("/somePath/thatIsNotThere");

            var routeAction = BotFrameworkHttpHandler.GetRoute(mockRequest.Object);

            Assert.Null(routeAction);
        }
    }
}
