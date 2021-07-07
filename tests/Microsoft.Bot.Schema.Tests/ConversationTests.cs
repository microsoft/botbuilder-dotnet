// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class ConversationTests
    {
        [Fact]
        public void ConversationAccountInits()
        {
            var isGroup = true;
            var conversationType = "convoType";
            var id = "myId";
            var name = "name";
            var aadObjectId = "aadObjectId";
            var role = "role";
            var tenantId = "tenantId";
            var props = new JObject();

            var convoAccount = new ConversationAccount(isGroup, conversationType, id, name, aadObjectId, role, tenantId)
            {
                Properties = props
            };

            Assert.NotNull(convoAccount);
            Assert.IsType<ConversationAccount>(convoAccount);
            Assert.Equal(isGroup, convoAccount.IsGroup);
            Assert.Equal(conversationType, convoAccount.ConversationType);
            Assert.Equal(id, convoAccount.Id);
            Assert.Equal(name, convoAccount.Name);
            Assert.Equal(aadObjectId, convoAccount.AadObjectId);
            Assert.Equal(role, convoAccount.Role);
            Assert.Equal(tenantId, convoAccount.TenantId);
            Assert.Equal(props, convoAccount.Properties);
        }

        [Fact]
        public void ConversationMembersInits()
        {
            var id = "myId";
            var members = new List<ChannelAccount>() { new ChannelAccount("id", "name", "role", "aadObjectId") };

            var convoMembers = new ConversationMembers(id, members);

            Assert.NotNull(convoMembers);
            Assert.IsType<ConversationMembers>(convoMembers);
            Assert.Equal(id, convoMembers.Id);
            Assert.Equal(members, convoMembers.Members);
        }

        [Fact]
        public void ConversationMembersInitsWithNoArgs()
        {
            var convoMembers = new ConversationMembers();

            Assert.NotNull(convoMembers);
            Assert.IsType<ConversationMembers>(convoMembers);
        }

        [Fact]
        public void ConversationParametersInits()
        {
            var isGroup = true;
            var bot = new ChannelAccount("botId", "botName", "botRole", "botAadObjectId");
            var members = new List<ChannelAccount>() { bot, new ChannelAccount("userId", "userName", "userRole", "userAadObjectId") };
            var topicName = "topicName";
            var activity = new Activity();
            var channelData = new { Data = "value" };
            var tenantId = "tenantId";

            var convoParameters = new ConversationParameters(isGroup, bot, members, topicName, activity, channelData, tenantId);

            Assert.NotNull(convoParameters);
            Assert.IsType<ConversationParameters>(convoParameters);
            Assert.Equal(isGroup, convoParameters.IsGroup);
            Assert.Equal(bot, convoParameters.Bot);
            Assert.Equal(members, convoParameters.Members);
            Assert.Equal(topicName, convoParameters.TopicName);
            Assert.Equal(activity, convoParameters.Activity);
            Assert.Equal(channelData, convoParameters.ChannelData);
            Assert.Equal(tenantId, convoParameters.TenantId);
        }

        [Fact]
        public void ConversationParametersInitsWithNoArgs()
        {
            var convoParameters = new ConversationParameters();

            Assert.NotNull(convoParameters);
            Assert.IsType<ConversationParameters>(convoParameters);
        }

        [Fact]
        public void ConversationReferenceInitsWithLocale()
        {
            var locale = new CultureInfo("es-es");
            var activityId = "activityId";
            var user = new ChannelAccount("userId", "userName", "userRole", "userAadObjectId");
            var bot = new ChannelAccount("botId", "botName", "botRole", "botAadObjectId");
            var conversation = new ConversationAccount();
            var channelId = "channelId";
            var serviceUrl = "http://myServiceUrl.com";

            var convoRef = new ConversationReference(locale, activityId, user, bot, conversation, channelId, serviceUrl);

            Assert.NotNull(convoRef);
            Assert.IsType<ConversationReference>(convoRef);
            Assert.Equal(locale.ToString(), convoRef.Locale);
            Assert.Equal(activityId, convoRef.ActivityId);
            Assert.Equal(user, convoRef.User);
            Assert.Equal(bot, convoRef.Bot);
            Assert.Equal(conversation, convoRef.Conversation);
            Assert.Equal(channelId, convoRef.ChannelId);
            Assert.Equal(serviceUrl, convoRef.ServiceUrl);
        }

        [Fact]
        public void ConversationReferenceInits()
        {
            var activityId = "activityId";
            var user = new ChannelAccount("userId", "userName", "userRole", "userAadObjectId");
            var bot = new ChannelAccount("botId", "botName", "botRole", "botAadObjectId");
            var conversation = new ConversationAccount();
            var channelId = "channelId";
            var serviceUrl = "http://myServiceUrl.com";

            var convoRef = new ConversationReference(activityId, user, bot, conversation, channelId, serviceUrl);

            Assert.NotNull(convoRef);
            Assert.IsType<ConversationReference>(convoRef);
            Assert.Equal(activityId, convoRef.ActivityId);
            Assert.Equal(user, convoRef.User);
            Assert.Equal(bot, convoRef.Bot);
            Assert.Equal(conversation, convoRef.Conversation);
            Assert.Equal(channelId, convoRef.ChannelId);
            Assert.Equal(serviceUrl, convoRef.ServiceUrl);
        }

        [Fact]
        public void ConversationReferenceInitsWithNoArgs()
        {
            var convoRef = new ConversationReference();

            Assert.NotNull(convoRef);
            Assert.IsType<ConversationReference>(convoRef);
        }

        [Fact]
        public void ConversationReferenceGetContinuationActivity()
        {
            var locale = new CultureInfo("es-es");
            var activityId = "activityId";
            var user = new ChannelAccount("userId", "userName", "userRole", "userAadObjectId");
            var bot = new ChannelAccount("botId", "botName", "botRole", "botAadObjectId");
            var conversation = new ConversationAccount();
            var channelId = "channelId";
            var serviceUrl = "http://myServiceUrl.com";
            var convoRef = new ConversationReference(locale, activityId, user, bot, conversation, channelId, serviceUrl);

            var continuationActivity = convoRef.GetContinuationActivity();

            Assert.NotNull(continuationActivity);
            Assert.IsType<Activity>(continuationActivity);
            Assert.Equal(ActivityEventNames.ContinueConversation, continuationActivity.Name);
            Assert.Equal(channelId, continuationActivity.ChannelId);
            Assert.Equal(locale.ToString(), continuationActivity.Locale);
            Assert.Equal(serviceUrl, continuationActivity.ServiceUrl);
        }

        [Fact]
        public void ConversationResourceResponseInits()
        {
            var activityId = "activityId";
            var serviceUrl = "http://MyServiceUrl.com";
            var id = "myId";

            var convoResourceResponse = new ConversationResourceResponse(activityId, serviceUrl, id);

            Assert.NotNull(convoResourceResponse);
            Assert.IsType<ConversationResourceResponse>(convoResourceResponse);
            Assert.Equal(activityId, convoResourceResponse.ActivityId);
            Assert.Equal(serviceUrl, convoResourceResponse.ServiceUrl);
            Assert.Equal(id, convoResourceResponse.Id);
        }

        [Fact]
        public void ConversationResourceResponseInitsWithNoArgs()
        {
            var convoResourceResponse = new ConversationResourceResponse();

            Assert.NotNull(convoResourceResponse);
            Assert.IsType<ConversationResourceResponse>(convoResourceResponse);
        }

        [Fact]
        public void ConversationResultInits()
        {
            var continuationToken = "continuationToken";
            var conversations = new List<ConversationMembers>()
            {
                new ConversationMembers("id1", new List<ChannelAccount>()),
                new ConversationMembers("id2", new List<ChannelAccount>())
            };

            var convosResult = new ConversationsResult(continuationToken, conversations);

            Assert.NotNull(convosResult);
            Assert.IsType<ConversationsResult>(convosResult);
            Assert.Equal(continuationToken, convosResult.ContinuationToken);
            Assert.Equal(conversations, convosResult.Conversations);
            Assert.Equal(2, convosResult.Conversations.Count);
        }

        [Fact]
        public void ConversationResultInitsWithNoArgs()
        {
            var convosResult = new ConversationsResult();

            Assert.NotNull(convosResult);
            Assert.IsType<ConversationsResult>(convosResult);
        }
    }
}
