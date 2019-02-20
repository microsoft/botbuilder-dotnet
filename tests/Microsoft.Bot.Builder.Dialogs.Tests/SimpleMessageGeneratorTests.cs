// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class MockLanguageGenator : ILanguageGenerator
    {
        public Task<string> Generate(string locale, string inlineTemplate, string id, object data, string[] types, string[] tags)
        {
            if (!String.IsNullOrEmpty(inlineTemplate))
            {
                return Task.FromResult(inlineTemplate);
            }
            else
            {
                return Task.FromResult(id);
            }
        }
    }

    [TestClass]
    public class SimpleMessageGeneratorTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task TestInline()
        {
            var lg = new MockLanguageGenator();
            var mg = new TextMessageActivityGenerator(lg);
            var activity = await mg.Generate("", "text", id: null, data: null, types: null, tags: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("text", activity.Text);
            Assert.AreEqual("text", activity.Speak);
        }

        [TestMethod]
        public async Task TestSpeak()
        {
            var lg = new MockLanguageGenator();
            var mg = new TextMessageActivityGenerator(lg);
            var activity = await mg.Generate("", "text||speak", id: null, data: null, types: null, tags: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("text", activity.Text);
            Assert.AreEqual("speak", activity.Speak);
        }

        [TestMethod]
        public async Task TestHerocard()
        {
            var lg = new MockLanguageGenator();
            var mg = new TextMessageActivityGenerator(lg);
            IMessageActivity activity = await mg.Generate("", "[Herocard\ntitle=test]", id: null, data: null, types: null, tags: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual("application/vnd.microsoft.card.hero", activity.Attachments[0].ContentType);
            var card = activity.Attachments[0].Content as HeroCard;
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("test", card.Title, "card title should be set");
            // TODO add all of the other property types
        }
    }
}
