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
            MessageActivity activity = DialogTestBase.MakeTestMessage();
            Assert.IsFalse(activity.HasContent());
            activity.Text = "test";
            Assert.IsTrue(activity.HasContent());

        }

        [TestMethod]
        public void GetMentions_Test()
        {
            MessageActivity activity = DialogTestBase.MakeTestMessage();
            Assert.IsFalse(activity.GetMentions().Any());
            activity.Entities = new List<Microsoft.Bot.Schema.Entity> { new Mention() { Text = "testMention" } };
            // Cloning activity to resemble the incoming activity to bot
            var clonedActivity = JsonConvert.DeserializeObject<MessageActivity>(JsonConvert.SerializeObject(activity));
            Assert.IsTrue(clonedActivity.GetMentions().Any());
            Assert.AreEqual("testMention", clonedActivity.GetMentions().First().Text);
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
