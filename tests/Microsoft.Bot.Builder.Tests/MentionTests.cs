// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class MentionTests
    {
        [Fact]
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

            Assert.Equal("sometext", activity.Text);
        }

        [Fact]
        public void Mention_Teams()
        {
            var mentionJson = "{\"mentioned\": {\"id\": \"recipientid\"},\"text\": \"<at>botname</at>\"}";
            var mention = JsonConvert.DeserializeObject<Entity>(mentionJson);
            mention.Type = "mention";

            var activity = MessageFactory.Text("<at>botname</at> sometext");
            activity.Entities.Add(mention);

            activity.RemoveMentionText("recipientid");

            Assert.Equal("sometext", activity.Text);
        }

        [Fact]
        public void Mention_slack()
        {
            var mentionJson = "{\"mentioned\": {\"id\": \"recipientid\"},\"text\": \"@botname\"}";
            var mention = JsonConvert.DeserializeObject<Entity>(mentionJson);
            mention.Type = "mention";

            var activity = MessageFactory.Text("@botname sometext");
            activity.Entities.Add(mention);

            activity.RemoveMentionText("recipientid");

            Assert.Equal("sometext", activity.Text);
        }

        [Fact]
        public void Mention_GroupMe()
        {
            var mentionJson = "{\"mentioned\": {\"id\": \"recipientid\"},\"text\": \"@bot name\"}";
            var mention = JsonConvert.DeserializeObject<Entity>(mentionJson);
            mention.Type = "mention";

            var activity = MessageFactory.Text("@bot name sometext");
            activity.Entities.Add(mention);

            activity.RemoveMentionText("recipientid");

            Assert.Equal("sometext", activity.Text);
        }

        [Fact]
        public void Mention_Telegram()
        {
            var mentionJson = "{\"mentioned\": {\"id\": \"recipientid\"},\"text\": \"botname\"}";
            var mention = JsonConvert.DeserializeObject<Entity>(mentionJson);
            mention.Type = "mention";

            var activity = MessageFactory.Text("botname sometext");
            activity.Entities.Add(mention);

            activity.RemoveMentionText("recipientid");

            Assert.Equal("sometext", activity.Text);
        }

        [Fact]
        public void Mention_Facebook()
        {
            // TODO: for now: Facebook mentions unknown at this time
        }

        [Fact]
        public void Mention_Email()
        {
            // TODO: for now: Email mentions not included in Activity.Text?
        }

        [Fact]
        public void Mention_Cortana()
        {
            // TODO: no-op for now: Cortana mentions unknown at this time
        }

        [Fact]
        public void Mention_Kik()
        {
            // TODO: for now: bot mentions in Kik don't get Entity info and not included in Activity.Text
        }

        [Fact]
        public void Mention_Twilio()
        {
            // TODO: no-op for now: Twilio mentions unknown at this time.  Could not determine if they are supported.
        }
    }
}
