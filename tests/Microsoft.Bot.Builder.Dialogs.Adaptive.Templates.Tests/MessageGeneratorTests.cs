using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Tests;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class MessageGeneratorTests
    {
        private static ResourceExplorer resourceExplorer;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            DeclarativeTypeLoader.AddComponent(new AdaptiveComponentRegistration());
            DeclarativeTypeLoader.AddComponent(new LanguageGenerationComponentRegistration());

            resourceExplorer = ResourceExplorer.LoadProject(GetProjectFolder());
        }

        [TestMethod]
        public async Task TestInlineActivityFactory()
        {
            var context = GetTurnContext(new MockLanguageGenerator());
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            var lgStringResult = await languageGenerator.Generate(context, "text", data: null).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);

            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("text", activity.Text);
            Assert.AreEqual("text", activity.Speak);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TestNotSupportStructuredType()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            var lgStringResult = await languageGenerator.Generate(context, "@{notSupport()}", null).ConfigureAwait(false);
            var result = ActivityFactory.CreateActivity(lgStringResult);
        }

        [TestMethod]
        public async Task TestHerocardWithCardAction()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgStringResult = await languageGenerator.Generate(context, "@{HerocardWithCardAction()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertCardActionActivity(activity);
        }

        [TestMethod]
        public async Task TestAdaptivecardActivity()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            var lgStringResult = await languageGenerator.Generate(context, "@{adaptivecardActivity()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertAdaptiveCardActivity(activity);
        }

        [TestMethod]
        public async Task TestExternalAdaptiveCardActivity()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            var lgStringResult = await languageGenerator.Generate(context, "@{externalAdaptiveCardActivity()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertAdaptiveCardActivity(activity);
        }

        [TestMethod]
        public async Task TestMultiExternalAdaptiveCardActivity()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.titles = new JArray() { "test0", "test1", "test2" };
            var lgStringResult = await languageGenerator.Generate(context, "@{multiExternalAdaptiveCardActivity()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertMultiAdaptiveCardActivity(activity);
        }

        [TestMethod]
        public async Task TestAdaptivecardActivityWithAttachmentStructure()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            var lgStringResult = await languageGenerator.Generate(context, "@{adaptivecardActivityWithAttachmentStructure()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertAdaptiveCardActivity(activity);
        }

        [TestMethod]
        public async Task TestExternalHeroCardActivity()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.type = "imBack";
            data.title = "taptitle";
            data.value = "tapvalue";
            var lgStringResult = await languageGenerator.Generate(context, "@{externalHeroCardActivity()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertActivityWithHeroCardAttachment(activity);
        }

        [TestMethod]
        public async Task TestEventActivity()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.text = "textContent";
            var lgStringResult = await languageGenerator.Generate(context, "@{eventActivity()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertEventActivity(activity);
        }

        [TestMethod]
        public async Task TestHandoffActivity()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.text = "textContent";
            var lgStringResult = await languageGenerator.Generate(context, "@{handoffActivity()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertHandoffActivity(activity);
        }

        [TestMethod]
        public async Task TestActivityWithHeroCardAttachment()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgStringResult = await languageGenerator.Generate(context, "@{activityWithHeroCardAttachment()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertActivityWithHeroCardAttachment(activity);
        }

        [TestMethod]
        public async Task TestHerocardAttachment()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.type = "imBack";
            data.title = "taptitle";
            data.value = "tapvalue";
            var lgStringResult = await languageGenerator.Generate(context, "@{herocardAttachment()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertActivityWithHeroCardAttachment(activity);
        }

        [TestMethod]
        public async Task TestHerocardActivityWithAttachmentStructure()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgStringResult = await languageGenerator.Generate(context, "@{activityWithMultiAttachments()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertActivityWithMultiAttachments(activity);
        }

        [TestMethod]
        public async Task TestActivityWithSuggestionActions()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgStringResult = await languageGenerator.Generate(context, "@{activityWithSuggestionActions()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertActivityWithSuggestionActions(activity);
        }

        [TestMethod]
        public async Task TestMessageActivityAll()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";

            var lgStringResult = await languageGenerator.Generate(context, "@{messageActivityAll()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertMessageActivityAll(activity);
        }

        [TestMethod]
        public async Task TestActivityWithMultiStructuredSuggestionActions()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.text = "textContent";
            var lgStringResult = await languageGenerator.Generate(context, "@{activityWithMultiStructuredSuggestionActions()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertActivityWithMultiStructuredSuggestionActions(activity);
        }

        [TestMethod]
        public async Task TestActivityWithMultiStringSuggestionActions()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.text = "textContent";
            var lgStringResult = await languageGenerator.Generate(context, "@{activityWithMultiStringSuggestionActions()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertActivityWithMultiStringSuggestionActions(activity);
        }

        [TestMethod]
        public async Task TestHeroCardTemplate()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.type = "herocard";
            var lgStringResult = await languageGenerator.Generate(context, "@{HeroCardTemplate()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertHeroCardActivity(activity);
        }

        [TestMethod]
        public async Task TestThumbnailCardTemplate()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.type = "thumbnailcard";
            var lgStringResult = await languageGenerator.Generate(context, "@{ThumbnailCardTemplate()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertThumbnailCardActivity(activity);
        }

        [TestMethod]
        public async Task TestAudioCardTemplate()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.type = "audiocard";
            var lgStringResult = await languageGenerator.Generate(context, "@{AudioCardTemplate()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertAudioCardActivity(activity);
        }

        [TestMethod]
        public async Task TestVideoCardTemplate()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.type = "videocard";
            var lgStringResult = await languageGenerator.Generate(context, "@{VideoCardTemplate()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertVideoCardActivity(activity);
        }

        [TestMethod]
        public async Task TestSigninCardTemplate()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.signinlabel = "Sign in";
            data.url = "https://login.microsoftonline.com/";
            var lgStringResult = await languageGenerator.Generate(context, "@{SigninCardTemplate()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertSigninCardActivity(activity);
        }

        [TestMethod]
        public async Task TestOAuthCardTemplate()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.signinlabel = "Sign in";
            data.url = "https://login.microsoftonline.com/";
            data.connectionName = "MyConnection";
            var lgStringResult = await languageGenerator.Generate(context, "@{OAuthCardTemplate()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertOAuthCardActivity(activity);
        }

        [TestMethod]
        public async Task TestReceiptCardTemplate()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            var data = new JObject
            {
                ["receiptItems"] = JToken.FromObject(new List<ReceiptItem>
                {
                    new ReceiptItem(
                        "Data Transfer",
                        price: "$ 38.45",
                        quantity: "368",
                        image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png")),
                    new ReceiptItem(
                        "App Service",
                        price: "$ 45.00",
                        quantity: "720",
                        image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png")),
                }),
                ["type"] = "ReceiptCard"
            };
            var lgStringResult = await languageGenerator.Generate(context, "@{ReceiptCardTemplate()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertReceiptCardActivity(activity);
        }

        [TestMethod]
        public async Task TestSuggestedActionsReference()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            dynamic data = new JObject();
            data.text = "textContent";
            var lgStringResult = await languageGenerator.Generate(context, "@{SuggestedActionsReference()}", data: data).ConfigureAwait(false);
            var activity = ActivityFactory.CreateActivity(lgStringResult);
            AssertSuggestedActionsReferenceActivity(activity);
        }

        private static string GetProjectFolder()
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
        }

        private void AssertSuggestedActionsReferenceActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("textContent", activity.Text, "should have right text");
           
            Assert.AreEqual(5, activity.SuggestedActions.Actions.Count, "should have 5 actions in suggestedAction");
            Assert.AreEqual("Add todo", activity.SuggestedActions.Actions[0].Value);
            Assert.AreEqual("View Todo", activity.SuggestedActions.Actions[1].Value);
            Assert.AreEqual("Remove Todo", activity.SuggestedActions.Actions[2].Value);
            Assert.AreEqual("Cancel", activity.SuggestedActions.Actions[3].Value);
            Assert.AreEqual("Help", activity.SuggestedActions.Actions[4].Value);
        }

        private void AssertMessageActivityAll(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("textContent", activity.Text);
            Assert.AreEqual("textContent", activity.Speak);
            Assert.AreEqual("accepting", activity.InputHint);
            var semanticAction = activity.SemanticAction;
            Assert.AreEqual("actionId", semanticAction.Id);
            Assert.AreEqual(1, semanticAction.Entities.Count);
            Assert.AreEqual(true, semanticAction.Entities.ContainsKey("key1"));
            Assert.AreEqual("entityType", semanticAction.Entities["key1"].Type);

            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(AttachmentLayoutTypes.List, activity.AttachmentLayout);
            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.IsNotNull(card, "should have herocard");
            var tap = card.Tap;
            Assert.AreEqual("taptitle", tap.Title, "tap title should be set");
            Assert.AreEqual("tapvalue", tap.Value, "tap value should be set");
            Assert.AreEqual("imBack", tap.Type, "tap type should be set");
            Assert.AreEqual("textContent", card.Text, "card text should be set");
            Assert.AreEqual(1, card.Buttons.Count, "card buttons should be set");
            Assert.AreEqual($"imBack", card.Buttons[0].Type, "card buttons should be set");
            Assert.AreEqual($"titleContent", card.Buttons[0].Title, "card buttons should be set");
            Assert.AreEqual($"textContent", card.Buttons[0].Value, "card buttons should be set");
            Assert.AreEqual(2, activity.SuggestedActions.Actions.Count);
            Assert.AreEqual("firstItem", activity.SuggestedActions.Actions[0].Title);
            Assert.AreEqual("firstItem", activity.SuggestedActions.Actions[0].Value);
            Assert.AreEqual("titleContent", activity.SuggestedActions.Actions[1].Title);
            Assert.AreEqual("textContent", activity.SuggestedActions.Actions[1].Value);
        }

        private void AssertActivityWithSuggestionActions(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("textContent", activity.Text);
            Assert.AreEqual(2, activity.SuggestedActions.Actions.Count);
            Assert.AreEqual("firstItem", activity.SuggestedActions.Actions[0].Title);
            Assert.AreEqual("firstItem", activity.SuggestedActions.Actions[0].Value);
            Assert.AreEqual("titleContent", activity.SuggestedActions.Actions[1].Title);
            Assert.AreEqual("textContent", activity.SuggestedActions.Actions[1].Value);
        }

        private void AssertActivityWithMultiAttachments(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(2, activity.Attachments.Count);
            Assert.AreEqual(ThumbnailCard.ContentType, activity.Attachments[1].ContentType);
            var card = ((JObject)activity.Attachments[1].Content).ToObject<ThumbnailCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("type", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url, "image should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[1].Url, "image should be set");
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }
        }

        private void AssertActivityWithHeroCardAttachment(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            var tap = card.Tap;
            Assert.AreEqual("taptitle", tap.Title, "tap title should be set");
            Assert.AreEqual("tapvalue", tap.Value, "tap value should be set");
            Assert.AreEqual("imBack", tap.Type, "tap type should be set");
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("titleContent", card.Title, "card title should be set");
            Assert.AreEqual("textContent", card.Text, "card text should be set");
            Assert.AreEqual(1, card.Buttons.Count, "card buttons should be set");
            Assert.AreEqual($"imBack", card.Buttons[0].Type, "card buttons should be set");
            Assert.AreEqual($"titleContent", card.Buttons[0].Title, "card buttons should be set");
            Assert.AreEqual($"textContent", card.Buttons[0].Value, "card buttons should be set");
        }

        private void AssertHandoffActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Handoff, activity.Type);
            Assert.AreEqual("textContent", activity.Name, "card name should be set");
            Assert.AreEqual("textContent", activity.Value, "card value should be set");
        }

        private void AssertEventActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Event, activity.Type);
            Assert.AreEqual("textContent", activity.Name, "card name should be set");
            Assert.AreEqual("textContent", activity.Value, "card value should be set");
        }

        private void AssertAdaptiveCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual("application/vnd.microsoft.card.adaptive", activity.Attachments[0].ContentType);
            Assert.AreEqual("test", (string)((dynamic)activity.Attachments[0].Content).body[0].text);
        }

        private void AssertMultiAdaptiveCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(3, activity.Attachments.Count);
            for (int i = 0; i < 3; ++i)
            {
                Assert.AreEqual("application/vnd.microsoft.card.adaptive", activity.Attachments[i].ContentType);
                Assert.AreEqual($"test{i}", (string)((dynamic)activity.Attachments[i].Content).body[0].text);
            }
        }

        private void AssertCardActionActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("titleContent", card.Title, "card title should be set");
            Assert.AreEqual("textContent", card.Text, "card text should be set");
            Assert.AreEqual(1, card.Buttons.Count, "card buttons should be set");
            Assert.AreEqual($"imBack", card.Buttons[0].Type, "card buttons should be set");
            Assert.AreEqual($"titleContent", card.Buttons[0].Title, "card buttons should be set");
            Assert.AreEqual($"textContent", card.Buttons[0].Value, "card buttons should be set");
        }

        private void AssertThumbnailCardActivity(Activity activity)
        {
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
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }
        }

        private void AssertHeroCardActivity(Activity activity)
        {
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
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }
        }

        private void AssertAudioCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(AudioCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<AudioCard>();
            Assert.IsNotNull(card, "should have audiocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("audiocard", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Image.Url, "image should be set");
            Assert.AreEqual("https://contoso.com/media/AllegrofromDuetinCMajor.mp3", card.Media[0].Url);
            Assert.AreEqual(false, card.Shareable);
            Assert.AreEqual(true, card.Autoloop);
            Assert.AreEqual(true, card.Autostart);
            Assert.AreEqual("16:9", card.Aspect);
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }
        }

        private void AssertVideoCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(VideoCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<VideoCard>();
            Assert.IsNotNull(card, "should have videocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("videocard", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Image.Url, "image should be set");
            Assert.AreEqual("https://youtu.be/530FEFogfBQ", card.Media[0].Url);
            Assert.AreEqual(false, card.Shareable);
            Assert.AreEqual(true, card.Autoloop);
            Assert.AreEqual(true, card.Autostart);
            Assert.AreEqual("16:9", card.Aspect);
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }
        }

        private void AssertSigninCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(SigninCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<SigninCard>();
            Assert.IsNotNull(card, "should have signincard");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual(1, card.Buttons.Count, "card buttons should be set");
            Assert.AreEqual($"Sign in", card.Buttons[0].Title);
            Assert.AreEqual(ActionTypes.Signin, card.Buttons[0].Type);
            Assert.AreEqual($"https://login.microsoftonline.com/", card.Buttons[0].Value);
        }

        private void AssertOAuthCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(OAuthCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<OAuthCard>();
            Assert.IsNotNull(card, "should have signincard");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("MyConnection", card.ConnectionName);
            Assert.AreEqual(1, card.Buttons.Count, "card buttons should be set");
            Assert.AreEqual($"Sign in", card.Buttons[0].Title);
            Assert.AreEqual(ActionTypes.Signin, card.Buttons[0].Type);
            Assert.AreEqual($"https://login.microsoftonline.com/", card.Buttons[0].Value);
        }

        private void AssertActivityWithMultiStructuredSuggestionActions(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("textContent", activity.Text);
            Assert.AreEqual(3, activity.SuggestedActions.Actions.Count);
            Assert.AreEqual("first suggestion", activity.SuggestedActions.Actions[0].Value);
            Assert.AreEqual("first suggestion", activity.SuggestedActions.Actions[0].Title);
            Assert.AreEqual("first suggestion", activity.SuggestedActions.Actions[0].Text);
            Assert.AreEqual("second suggestion", activity.SuggestedActions.Actions[1].Value);
            Assert.AreEqual("second suggestion", activity.SuggestedActions.Actions[1].Title);
            Assert.AreEqual("second suggestion", activity.SuggestedActions.Actions[1].Text);
            Assert.AreEqual("third suggestion", activity.SuggestedActions.Actions[2].Value);
            Assert.AreEqual("third suggestion", activity.SuggestedActions.Actions[2].Title);
            Assert.AreEqual("third suggestion", activity.SuggestedActions.Actions[2].Text);
        }

        private void AssertActivityWithMultiStringSuggestionActions(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("textContent", activity.Text);
            Assert.AreEqual(3, activity.SuggestedActions.Actions.Count);
            Assert.AreEqual("first suggestion", activity.SuggestedActions.Actions[0].Value);
            Assert.AreEqual("first suggestion", activity.SuggestedActions.Actions[0].Title);
            Assert.AreEqual("second suggestion", activity.SuggestedActions.Actions[1].Value);
            Assert.AreEqual("second suggestion", activity.SuggestedActions.Actions[1].Title);
            Assert.AreEqual("third suggestion", activity.SuggestedActions.Actions[2].Value);
            Assert.AreEqual("third suggestion", activity.SuggestedActions.Actions[2].Title);
        }

        private void AssertReceiptCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(ReceiptCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<ReceiptCard>();
            Assert.IsNotNull(card, "should have ReceiptCard");
            Assert.AreEqual("John Doe", card.Title);
            Assert.AreEqual("$ 7.50", card.Tax);
            Assert.AreEqual("$ 90.95", card.Total);
            var buttons = card.Buttons;

            Assert.AreEqual(1, buttons.Count, "should have a button");
            Assert.AreEqual(ActionTypes.OpenUrl, buttons[0].Type);
            Assert.AreEqual("More information", buttons[0].Title);
            Assert.AreEqual("https://account.windowsazure.com/content/6.10.1.38-.8225.160809-1618/aux-pre/images/offer-icon-freetrial.png", buttons[0].Image);
            Assert.AreEqual("https://azure.microsoft.com/en-us/pricing/", buttons[0].Value);

            var facts = card.Facts;
            Assert.AreEqual(2, facts.Count, "should have 2 facts");
            Assert.AreEqual("Order Number", facts[0].Key);
            Assert.AreEqual("1234", facts[0].Value);
            Assert.AreEqual("Payment Method", facts[1].Key);
            Assert.AreEqual("VISA 5555-****", facts[1].Value);

            var items = card.Items;
            Assert.AreEqual(2, items.Count, "should have 2 items");
            Assert.AreEqual("Data Transfer", items[0].Title);
            Assert.AreEqual("https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png", items[0].Image.Url);
            Assert.AreEqual("$ 38.45", items[0].Price);
            Assert.AreEqual("368", items[0].Quantity);
            Assert.AreEqual("App Service", items[1].Title);
            Assert.AreEqual("https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png", items[1].Image.Url);
            Assert.AreEqual("$ 45.00", items[1].Price);
            Assert.AreEqual("720", items[1].Quantity);
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
            var lgresource = resourceExplorer.GetResource(lgFile) as FileResource;
            context.TurnState.Add<ILanguageGenerator>(new TemplateEngineLanguageGenerator(lgresource.FullName, MultiLanguageResourceLoader.Load(resourceExplorer)));

            return context;
        }

        private string GetLGTFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "lg", fileName);
        }
    }
}
