// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class MockLanguageGenator : ILanguageGenerator
    {
        public Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            return Task.FromResult(template);
        }
    }

    [TestClass]
    public class TextMessageGeneratorTests
    {
        public TestContext TestContext { get; set; }

        private static ResourceExplorer resourceExplorer;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            TypeFactory.RegisterAdaptiveTypes();
            resourceExplorer = ResourceExplorer.LoadProject(GetProjectFolder());
        }
        private static string GetProjectFolder()
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
        }


        [ClassCleanup]
        public static void ClassCleanup()
        {
            resourceExplorer.Dispose();
        }

        private ITurnContext GetTurnContext(ILanguageGenerator lg)
        {
            var context = new TurnContext(new TestAdapter(), new Activity());
            context.TurnState.Add<ILanguageGenerator>(lg);
            return context;
        }

        private async Task<ITurnContext> GetTurnContext(string lgFile)
        {

            var context = new TurnContext(new TestAdapter(), new Activity());
            var lgText = await resourceExplorer.GetResource(lgFile).ReadTextAsync();
            context.TurnState.Add<ILanguageGenerator>(new TemplateEngineLanguageGenerator(lgText, "test", LanguageGeneratorManager.ResourceResolver(resourceExplorer)));
            return context;
        }


        [TestMethod]
        public async Task TestInline()
        {
            var context = GetTurnContext(new MockLanguageGenator());
            var mg = new TextMessageActivityGenerator();
            var activity = await mg.Generate(context, "text", data: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("text", activity.Text);
            Assert.AreEqual("text", activity.Speak);
        }

        [TestMethod]
        public async Task TestSpeak()
        {
            var context = GetTurnContext(new MockLanguageGenator());
            var mg = new TextMessageActivityGenerator();
            var activity = await mg.Generate(context, "text||speak", data: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("text", activity.Text);
            Assert.AreEqual("speak", activity.Speak);
        }

        [TestMethod]
        public async Task TestHerocard()
        {
            var context = await GetTurnContext("NonAdaptiveCardActivity.lg");
            var mg = new TextMessageActivityGenerator();
            dynamic data = new JObject();
            data.type = "herocard";
            IMessageActivity activity = await mg.Generate(context, "[HeroCardTemplate]", data: data);
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
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[1].Url, "image should be set");
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            // TODO add all of the other property types
        }

        [TestMethod]
        public async Task TestThmbnailCard()
        {
            var context = await GetTurnContext("NonAdaptiveCardActivity.lg");
            var mg = new TextMessageActivityGenerator();
            dynamic data = new JObject();
            data.type = "thumbnailcard";
            IMessageActivity activity = await mg.Generate(context, "[ThumbnailCardTemplate]", data: data);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(ThumbnailCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<ThumbnailCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("thumbnailcard", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url, "image should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[1].Url, "image should be set");
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
            var gen = new TextMessageActivityGenerator();
            var activity = await gen.CreateActivityFromText(null, text);

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
            var context = await GetTurnContext("NonAdaptiveCardActivity.lg");
            var mg = new TextMessageActivityGenerator();

            IMessageActivity activity = await mg.Generate(context, "[ImageAttachment]", data: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(null, activity.Attachments[0].ContentType);
            Assert.AreEqual(null, activity.AttachmentLayout);
            Assert.AreEqual("https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png", activity.Attachments[0].ContentUrl);

            activity = await mg.Generate(context, "[ImageAttachment2]", data: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual("image/png", activity.Attachments[0].ContentType);
            Assert.AreEqual(AttachmentLayoutTypes.List, activity.AttachmentLayout);
            Assert.AreEqual("https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png", activity.Attachments[0].ContentUrl);
        }


        [TestMethod]
        public async Task TestLocalImageAttachment()
        {
            var context = await GetTurnContext("NonAdaptiveCardActivity.lg");
            var mg = new TextMessageActivityGenerator();

            IMessageActivity activity = await mg.Generate(context, "[ImageAttachmentLocal]", data: null);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual("image/jpg", activity.Attachments[0].ContentType);
            Assert.IsTrue(activity.Attachments[0].ContentUrl.StartsWith("data:"));
            var content = activity.Attachments[0].ContentUrl.Substring(activity.Attachments[0].ContentUrl.IndexOf("base64, ") + 8);
            var bytes = Convert.FromBase64String(content);
            Assert.AreEqual(237449, bytes.Length);
        }


        [TestMethod]
        public async Task TestAdaptiveCard()
        {
            var context = await GetTurnContext("AdaptiveCardActivity.lg");
            var mg = new TextMessageActivityGenerator();
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            IMessageActivity activity = await mg.Generate(context, "[adaptiveCardTemplate]", data: data);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual("application/vnd.microsoft.card.adaptive", activity.Attachments[0].ContentType);
            Assert.AreEqual("test", (string)((dynamic)activity.Attachments[0].Content).body[0].text);
        }

        [TestMethod]
        public async Task TextExternalAdaptiveCard()
        {
            var context = await GetTurnContext("AdaptiveCardActivity.lg");
            var mg = new TextMessageActivityGenerator();
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            IMessageActivity activity = await mg.Generate(context, "[externalAdaptiveCardTemplate]", data: data);
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
            var context = await GetTurnContext("NonAdaptiveCardActivity.lg");
            var mg = new TextMessageActivityGenerator();
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            IMessageActivity activity = await mg.Generate(context, "[AttachmentsTest]", data: data);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("Enjoy these pictures!", activity.Text.Trim());
            Assert.AreEqual("Enjoy <emphasize>these</emphasize> pictures!", activity.Speak.Trim());
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
            var context = await GetTurnContext("multiCards.lg");
            var mg = new TextMessageActivityGenerator();
            dynamic data = new JObject();
            data.type = "herocard";
            data.adaptiveCardTitle = "test";
            IMessageActivity activity = await mg.Generate(context, "[multiCardTemplate]", data: data);
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
            var context = await GetTurnContext("multiCards.lg");
            var mg = new TextMessageActivityGenerator();
            dynamic data = new JObject();
            data.url = "https://avatars0.githubusercontent.com/u/17789481?v";
            IMessageActivity activity = await mg.Generate(context, "```[Attachment=@{url} image/png]```", data: data);
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual("image/png", activity.Attachments[0].ContentType);
            Assert.AreEqual(data.url.ToString(), activity.Attachments[0].ContentUrl.ToString());
        }

    }
}
