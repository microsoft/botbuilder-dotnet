// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Mention")]
    public class MentionTests
    {
        [TestMethod]
        public void Mention_Skype()
        {
            // A Skype mention contains the user mention enclosed in <at> tags.  But the Activity.Text (as below)
            // does not.
            var mentionJson = "{\"mentioned\": {\"id\": \"recipientid\"},\"text\": \"<at id=\\\"28: 841caffa-9e92-425d-8d84-b503b3ded285\\\">botname</at>\"}";
            var mention = JsonConvert.DeserializeObject<Entity>(mentionJson);
            mention.Type = "mention";

            var activity = MessageFactory.Text("botname sometext");
            activity.ChannelId = "skype";
            activity.Entities.Add(mention);

            // Normalize the Skype mention so that it is in a format RemoveMentionText can handle.
            // If SkypeMentionNormalizeMiddleware is added to the adapters Middleware set, this
            // will be called on every Skype message.
            SkypeMentionNormalizeMiddleware.NormalizeSkypMentionText(activity);

            // This will remove the Mention.Text from the Activity.Text.  This should just leave before/after the
            // mention.
            activity.RemoveMentionText("recipientid");

            Assert.AreEqual(activity.Text, "sometext");
        }

        [TestMethod]
        public void Mention_Teams()
        {
            var mentionJson = "{\"mentioned\": {\"id\": \"recipientid\"},\"text\": \"<at>botname</at>\"}";
            var mention = JsonConvert.DeserializeObject<Entity>(mentionJson);
            mention.Type = "mention";

            var activity = MessageFactory.Text("<at>botname</at> sometext");
            activity.Entities.Add(mention);

            activity.RemoveMentionText("recipientid");

            Assert.AreEqual(activity.Text, "sometext");
        }

        [TestMethod]
        public void Mention_slack()
        {
            var mentionJson = "{\"mentioned\": {\"id\": \"recipientid\"},\"text\": \"@botname\"}";
            var mention = JsonConvert.DeserializeObject<Entity>(mentionJson);
            mention.Type = "mention";

            var activity = MessageFactory.Text("@botname sometext");
            activity.Entities.Add(mention);

            activity.RemoveMentionText("recipientid");

            Assert.AreEqual(activity.Text, "sometext");
        }

        [TestMethod]
        public void Mention_GroupMe()
        {
            var mentionJson = "{\"mentioned\": {\"id\": \"recipientid\"},\"text\": \"@bot name\"}";
            var mention = JsonConvert.DeserializeObject<Entity>(mentionJson);
            mention.Type = "mention";

            var activity = MessageFactory.Text("@bot name sometext");
            activity.Entities.Add(mention);

            activity.RemoveMentionText("recipientid");

            Assert.AreEqual(activity.Text, "sometext");
        }

        [TestMethod]
        public void Mention_Telegram()
        {
            var mentionJson = "{\"mentioned\": {\"id\": \"recipientid\"},\"text\": \"botname\"}";
            var mention = JsonConvert.DeserializeObject<Entity>(mentionJson);
            mention.Type = "mention";

            var activity = MessageFactory.Text("botname sometext");
            activity.Entities.Add(mention);

            activity.RemoveMentionText("recipientid");

            Assert.AreEqual(activity.Text, "sometext");
        }

        [TestMethod]
        public void Mention_Facebook()
        {
            // no-op for now: Facebook mentions unknown at this time
        }

        [TestMethod]
        public void Mention_Email()
        {
            // no-op for now: EMail mentions not included in Activity.Text?
        }

        [TestMethod]
        public void Mention_Cortana()
        {
            // no-op for now: Cortana mentions unknown at this time
        }

        [TestMethod]
        public void Mention_Kik()
        {
            // no-op for now: bot mentions in Kik don't get Entity info and not included in Activity.Text
        }

        [TestMethod]
        public void Mention_Twilio()
        {
            // no-op for now: Twilio mentions unknown at this time.  Could not determine if they are supported.
        }
    }
}
