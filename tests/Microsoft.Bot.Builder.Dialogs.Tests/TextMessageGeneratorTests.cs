// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class MockLanguageGenator : ILanguageGenerator
    {
        public Task<string> Generate(string locale, string inlineTemplate, string id, object data, string[] types, string[] tags, Func<string, object, object> valueBinder)
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
    public class TextMessageGeneratorTests
    {
        public TestContext TestContext { get; set; }

        private TextMessageActivityGenerator GetGenerator()
        {
            var rm = new BotResourceManager();
            rm.AddFolderResources(GetLgFolder());
            var lg = new LGLanguageGenerator(rm);
            var mg = new TextMessageActivityGenerator(lg);
            return mg;
        }


        private string GetLgFolder()
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "lg";
        }

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
            var mg = GetGenerator();
            dynamic data = new JObject();
            data.type = "herocard";
            IMessageActivity activity = await mg.Generate("", "[HeroCardTemplate]", id: null, data: data, types: null, tags: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("herocard", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url, "image should be set");
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            // TODO add all of the other property types
        }

        [TestMethod]
        public async Task TestCreateFromText()
        {
            await Task.Delay(0);
            string text = @"[Herocard 
    title=Cheese gromit! 
    subtitle=cheezy
    text=This is some text describing the card, it's cool because it's cool 
    images=https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg 
    buttons=Option 1| Option 2| Option 3]";
            var activity = TextMessageActivityGenerator.CreateActivityFromText(text);

            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("cheezy", card.Subtitle, "card subtitle should be set");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url, "image should be set");
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            // TODO add all of the other property types
        }


        [TestMethod]
        public async Task TestImageAttachment()
        {
            var mg = GetGenerator();

            IMessageActivity activity = await mg.Generate("", "[ImageAttachment]", id: null, data: null, types: null, tags: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(null, activity.Attachments[0].ContentType);
            Assert.AreEqual(null, activity.AttachmentLayout);
            Assert.AreEqual("https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png", activity.Attachments[0].ContentUrl);

            activity = await mg.Generate("", "[ImageAttachment2]", id: null, data: null, types: null, tags: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual("image/png", activity.Attachments[0].ContentType);
            Assert.AreEqual(AttachmentLayoutTypes.List, activity.AttachmentLayout);
            Assert.AreEqual("https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png", activity.Attachments[0].ContentUrl);
        }


        [TestMethod]
        public async Task TestAdaptiveCard()
        {
            var mg = GetGenerator();
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            IMessageActivity activity = await mg.Generate("", "[adaptiveCardTemplate]", id: null, data: data, types: null, tags: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual("application/vnd.microsoft.card.adaptive", activity.Attachments[0].ContentType);
            Assert.AreEqual("test", (string)((dynamic)activity.Attachments[0].Content).body[0].text);
        }
        

        [TestMethod]
        public async Task TestMultipleAttachments()
        {
            var mg = GetGenerator();
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            IMessageActivity activity = await mg.Generate("", "[AttachmentsTest]", id: null, data: data, types: null, tags: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("Enjoy these pictures!", activity.Text);
            Assert.AreEqual("Enjoy <emphasize>these</emphasize> pictures!", activity.Speak);
            Assert.AreEqual(AttachmentLayoutTypes.Carousel, activity.AttachmentLayout);
            Assert.AreEqual(4, activity.Attachments.Count);
            Assert.AreEqual("http://4.bp.blogspot.com/--cFa6t-x4qY/UAqEgUvPd2I/AAAAAAAANIg/pMLE080Zjh4/s1600/turtle.jpg", (string)((dynamic)activity.Attachments[0].ContentUrl));
            Assert.AreEqual("http://viagemempauta.com.br/wp-content/uploads/2015/09/2_All-Angle-By-Andreza-dos-Santos_FTS_2914-344-620x415.jpg", (string)((dynamic)activity.Attachments[1].ContentUrl));
            Assert.AreEqual("http://images.fineartamerica.com/images-medium-large/good-morning-turtles-freund-gloria.jpg", (string)((dynamic)activity.Attachments[2].ContentUrl));
            Assert.AreEqual("http://4.bp.blogspot.com/--cFa6t-x4qY/UAqEgUvPd2I/AAAAAAAANIg/pMLE080Zjh4/s1600/turtle.jpg", (string)((dynamic)activity.Attachments[3].ContentUrl));
        }

        [TestMethod]
        public async Task TestMultipleCards()
        {
            var mg = GetGenerator();
            dynamic data = new JObject();
            data.type = "herocard";
            data.adaptiveCardTitle = "test";
            IMessageActivity activity = await mg.Generate("", "[multiCardTemplate]", id: null, data: data, types: null, tags: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(4, activity.Attachments.Count);
            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            Assert.AreEqual(AdaptiveCard.ContentType, activity.Attachments[1].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("herocard", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url, "image should be set");
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");

            Assert.AreEqual("test", (string)((dynamic)activity.Attachments[1].Content).body[0].text);
        }


        [TestMethod]
        public async Task TestAttachmentContentType()
        {
            var mg = GetGenerator();
            dynamic data = new JObject();
            data.url = "https://avatars0.githubusercontent.com/u/17789481?v";
            IMessageActivity activity = await mg.Generate("", "```[Attachment=@{url} image/png]```", id: null, data: data, types: null, tags: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual("image/png", activity.Attachments[0].ContentType);
            Assert.AreEqual(data.url.ToString(), activity.Attachments[0].ContentUrl.ToString());
        }

    }
}
