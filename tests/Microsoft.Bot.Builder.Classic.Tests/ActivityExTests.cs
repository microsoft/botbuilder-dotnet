using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public class ActivityExTests
    {
        [TestMethod]
        public void HasContent_Test()
        {
            IMessageActivity activity = DialogTestBase.MakeTestMessage();
            Assert.IsFalse(activity.HasContent());
            activity.Text = "test";
            Assert.IsTrue(activity.HasContent());

        }

        [TestMethod]
        public void GetMentions_Test()
        {
            IMessageActivity activity = DialogTestBase.MakeTestMessage();
            Assert.IsFalse(activity.GetMentions().Any());
            activity.Entities = new List<Microsoft.Bot.Schema.Entity> { new Mention() { Text = "testMention" } };
            // Cloning activity to resemble the incoming activity to bot
            var clonedActivity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
            Assert.IsTrue(clonedActivity.GetMentions().Any());
            Assert.AreEqual("testMention", clonedActivity.GetMentions()[0].Text);
        }

        [TestMethod]
        public void TypeCastingsWork()
        {
            Activity activity = new Activity(type: ActivityTypes.ContactRelationUpdate);
            Assert.IsNotNull(activity.AsContactRelationUpdateActivity());
            Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.ConversationUpdate);
            Assert.IsNotNull(activity.AsConversationUpdateActivity());
            Assert.IsNull(activity.AsMessageActivity());

            //activity = new Activity(type: ActivityTypes.DeleteUserData);
            //Assert.IsNotNull(activity.AsDeleteUserDataActivity());
            //Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.EndOfConversation);
            Assert.IsNotNull(activity.AsEndOfConversationActivity());
            Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.Event);
            Assert.IsNotNull(activity.AsEventActivity());
            Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.InstallationUpdate);
            Assert.IsNotNull(activity.AsInstallationUpdateActivity());
            Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.Invoke);
            Assert.IsNotNull(activity.AsInvokeActivity());
            Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.Message);
            Assert.IsNotNull(activity.AsMessageActivity());
            Assert.IsNull(activity.AsEventActivity());

            activity = new Activity(type: ActivityTypes.MessageDelete);
            Assert.IsNotNull(activity.AsMessageDeleteActivity());
            Assert.IsNull(activity.AsEventActivity());

            activity = new Activity(type: ActivityTypes.MessageReaction);
            Assert.IsNotNull(activity.AsMessageReactionActivity());
            Assert.IsNull(activity.AsEventActivity());

            activity = new Activity(type: ActivityTypes.MessageUpdate);
            Assert.IsNotNull(activity.AsMessageUpdateActivity());
            Assert.IsNull(activity.AsEventActivity());

            //activity = new Activity(type: ActivityTypes.Ping);
            //Assert.IsNotNull(activity.AsPingActivity());

            activity = new Activity(type: ActivityTypes.Suggestion);
            Assert.IsNotNull(activity.AsSuggestionActivity());
            Assert.IsNull(activity.AsEventActivity());

            activity = new Activity(type: ActivityTypes.Typing);
            Assert.IsNotNull(activity.AsTypingActivity());
            Assert.IsNull(activity.AsEventActivity());
        }

        [TestMethod]
        public void TypeCastingsFromInterfaceWork()
        {
            IActivity activity = new Activity(type: ActivityTypes.ContactRelationUpdate);
            Assert.IsNotNull(activity.AsContactRelationUpdateActivity());
            Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.ConversationUpdate);
            Assert.IsNotNull(activity.AsConversationUpdateActivity());
            Assert.IsNull(activity.AsMessageActivity());

            //activity = new Activity(type: ActivityTypes.DeleteUserData);
            //Assert.IsNotNull(activity.AsDeleteUserDataActivity());
            //Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.EndOfConversation);
            Assert.IsNotNull(activity.AsEndOfConversationActivity());
            Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.Event);
            Assert.IsNotNull(activity.AsEventActivity());
            Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.InstallationUpdate);
            Assert.IsNotNull(activity.AsInstallationUpdateActivity());
            Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.Invoke);
            Assert.IsNotNull(activity.AsInvokeActivity());
            Assert.IsNull(activity.AsMessageActivity());

            activity = new Activity(type: ActivityTypes.Message);
            Assert.IsNotNull(activity.AsMessageActivity());
            Assert.IsNull(activity.AsEventActivity());

            activity = new Activity(type: ActivityTypes.MessageDelete);
            Assert.IsNotNull(activity.AsMessageDeleteActivity());
            Assert.IsNull(activity.AsEventActivity());

            activity = new Activity(type: ActivityTypes.MessageReaction);
            Assert.IsNotNull(activity.AsMessageReactionActivity());
            Assert.IsNull(activity.AsEventActivity());

            activity = new Activity(type: ActivityTypes.MessageUpdate);
            Assert.IsNotNull(activity.AsMessageUpdateActivity());
            Assert.IsNull(activity.AsEventActivity());

            //activity = new Activity(type: ActivityTypes.Ping);
            //Assert.IsNotNull(activity.AsPingActivity());

            activity = new Activity(type: ActivityTypes.Suggestion);
            Assert.IsNotNull(activity.AsSuggestionActivity());
            Assert.IsNull(activity.AsEventActivity());

            activity = new Activity(type: ActivityTypes.Typing);
            Assert.IsNotNull(activity.AsTypingActivity());
            Assert.IsNull(activity.AsEventActivity());
        }

        [TestMethod]
        public void TestChannelData()
        {
            Mention mention;
            string str = "test";
            Activity x = new Activity();
            x.ChannelData = new Mention() { Mentioned = new ChannelAccount() { Id = "1", Name = "Bob" }, Text = "123" };

            try
            {
                x.GetChannelData<string>();
                Assert.Fail("Should have thrown exception");
            }
            catch { }
            Assert.IsNotNull(x.GetChannelData<Mention>());
            Assert.IsFalse(x.TryGetChannelData<string>(out str));
            Assert.IsNull(str);
            Assert.IsTrue(x.TryGetChannelData<Mention>(out mention));
            Assert.IsNotNull(mention);

            str = "test";
            x = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(x));

            try
            {
                x.GetChannelData<string>();
                Assert.Fail("Should have thrown exception after deserialized");
            }
            catch { }
            Assert.IsNotNull(x.GetChannelData<Mention>());
            Assert.IsFalse(x.TryGetChannelData<string>(out str));
            Assert.IsNull(str);
            Assert.IsTrue(x.TryGetChannelData<Mention>(out mention));
            Assert.IsNotNull(mention);
        }
    }
}
