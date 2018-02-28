using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Tests
{
    public static class InteractionTestsActivityHelper
    {
        public static ConversationReference CreateConversationReference(
            string userName = "User1", string conversationName = "Convo1")
        {
            var conversationReference = new ConversationReference
            {
                ChannelId = "test",
                ServiceUrl = "https://test.com"
            };
            conversationReference.User = new ChannelAccount(userName.ToLower(), userName);
            conversationReference.Bot = new ChannelAccount("bot", "Bot");
            conversationReference.Conversation = new ConversationAccount(
                false, conversationName.ToLower(), conversationName);
            return conversationReference;
        }

        public static IEnumerable<IActivity> StartConversation(ConversationReference reference)
        {
            yield return new Activity()
            {
                Type = ActivityTypes.ConversationUpdate,
                Conversation = reference.Conversation,
                From = reference.User,
                Recipient = reference.Bot,
                ServiceUrl = reference.ServiceUrl,
                MembersAdded = new List<ChannelAccount> { reference.User },
                MembersRemoved = new List<ChannelAccount>()
            };
            yield return new Activity()
            {
                Type = ActivityTypes.ConversationUpdate,
                Conversation = reference.Conversation,
                From = reference.User,
                Recipient = reference.Bot,
                ServiceUrl = reference.ServiceUrl,
                MembersAdded = new List<ChannelAccount> { reference.Bot },
                MembersRemoved = new List<ChannelAccount>()
            };
        }

        public static IActivity CreateMessageActivity(ConversationReference reference, string message)
        {
            return new Activity()
            {
                Type = ActivityTypes.Message,
                Conversation = reference.Conversation,
                From = reference.User,
                Recipient = reference.Bot,
                ServiceUrl = reference.ServiceUrl,
                Text = message
            };
        }

        public static IActivity CreateEndOfConversation(ConversationReference reference, string endOfConversationCode = EndOfConversationCodes.Unknown)
        {
            return new Activity()
            {
                Type = ActivityTypes.EndOfConversation,
                Conversation = reference.Conversation,
                From = reference.User,
                Recipient = reference.Bot,
                ServiceUrl = reference.ServiceUrl,
                Code = endOfConversationCode
            };
        }

        public static IActivity CreateDeleteUserData(ConversationReference reference)
        {
            return new Activity()
            {
                Type = ActivityTypes.DeleteUserData,
                Conversation = reference.Conversation,
                From = reference.User,
                Recipient = reference.Bot,
                ServiceUrl = reference.ServiceUrl
            };
        }

        public static IList<IActivity> StartNewConversation()
        {
            var convo1 = CreateConversationReference(conversationName: "Convo1");
            var convo2 = CreateConversationReference(conversationName: "Convo2");
            var userAccount = convo1.User;
            var botAccount = convo1.Bot;

            var activities = new List<IActivity>();
            activities.AddRange(StartConversation(convo1));
            activities.Add(CreateMessageActivity(convo1, "set"));
            activities.Add(CreateEndOfConversation(convo1));
            activities.AddRange(StartConversation(convo2));

            return activities;
        }

        public static IList<IActivity> SetUpThenSendDeleteUserData()
        {
            var convo1 = CreateConversationReference(conversationName: "Convo1");
            var convo2 = CreateConversationReference(conversationName: "Convo2");
            var userAccount = convo1.User;
            var botAccount = convo1.Bot;

            var activities = new List<IActivity>();
            activities.AddRange(StartConversation(convo1));
            activities.Add(CreateMessageActivity(convo1, "set"));
            activities.Add(CreateDeleteUserData(convo1));

            return activities;
        }
    }
}
