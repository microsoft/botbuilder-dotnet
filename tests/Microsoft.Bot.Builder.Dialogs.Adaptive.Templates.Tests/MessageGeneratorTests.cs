using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
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
        public void TestInlineActivityFactory()
        {
            var lgResult = GetLGFile().Evaluate("text").ToString();
            var activity = ActivityFactory.CreateActivity(lgResult);

            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("text", activity.Text);
            Assert.AreEqual("text", activity.Speak);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestNotSupportStructuredType()
        {
            var lgResult = GetLGFile().EvaluateTemplate("notSupport");
            var activity = ActivityFactory.CreateActivity(lgResult);
        }

        [TestMethod]
        public void TestHerocardWithCardAction()
        {
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgResult = GetLGFile().EvaluateTemplate("HerocardWithCardAction", data).ToString();
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertCardActionActivity(activity);
        }

        [TestMethod]
        public void TestAdaptivecardActivity()
        {
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            var lgResult = GetLGFile().EvaluateTemplate("adaptivecardActivity", data).ToString();
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertAdaptiveCardActivity(activity);
        }

        [TestMethod]
        public void TestExternalAdaptiveCardActivity()
        {
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            var lgResult = GetLGFile().EvaluateTemplate("externalAdaptiveCardActivity", data).ToString();
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertAdaptiveCardActivity(activity);
        }

        [TestMethod]
        public void TestMultiExternalAdaptiveCardActivity()
        {
            dynamic data = new JObject();
            data.titles = new JArray() { "test0", "test1", "test2" };
            var lgResult = GetLGFile().EvaluateTemplate("multiExternalAdaptiveCardActivity", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertMultiAdaptiveCardActivity(activity);
        }

        [TestMethod]
        public void TestAdaptivecardActivityWithAttachmentStructure()
        {
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            var lgResult = GetLGFile().EvaluateTemplate("adaptivecardActivityWithAttachmentStructure", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertAdaptiveCardActivity(activity);
        }

        [TestMethod]
        public void TestExternalHeroCardActivity()
        {
            dynamic data = new JObject();
            data.type = "imBack";
            data.title = "taptitle";
            data.value = "tapvalue";
            var lgResult = GetLGFile().EvaluateTemplate("externalHeroCardActivity", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertActivityWithHeroCardAttachment(activity);
        }

        [TestMethod]
        public void TestEventActivity()
        {
            dynamic data = new JObject();
            data.text = "textContent";
            var lgResult = GetLGFile().EvaluateTemplate("eventActivity", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertEventActivity(activity);
        }

        [TestMethod]
        public void TestHandoffActivity()
        {
            dynamic data = new JObject();
            data.text = "textContent";
            var lgResult = GetLGFile().EvaluateTemplate("handoffActivity", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertHandoffActivity(activity);
        }

        [TestMethod]
        public void TestActivityWithHeroCardAttachment()
        {
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgResult = GetLGFile().EvaluateTemplate("activityWithHeroCardAttachment", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertActivityWithHeroCardAttachment(activity);
        }

        [TestMethod]
        public void TestHerocardAttachment()
        {
            dynamic data = new JObject();
            data.type = "imBack";
            data.title = "taptitle";
            data.value = "tapvalue";
            var lgResult = GetLGFile().EvaluateTemplate("herocardAttachment", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertActivityWithHeroCardAttachment(activity);
        }

        [TestMethod]
        public void TestHerocardActivityWithAttachmentStructure()
        {
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgResult = GetLGFile().EvaluateTemplate("activityWithMultiAttachments", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertActivityWithMultiAttachments(activity);
        }

        [TestMethod]
        public void TestActivityWithSuggestionActions()
        {
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgResult = GetLGFile().EvaluateTemplate("activityWithSuggestionActions", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertActivityWithSuggestionActions(activity);
        }

        [TestMethod]
        public void TestMessageActivityAll()
        {
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgResult = GetLGFile().EvaluateTemplate("messageActivityAll", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertMessageActivityAll(activity);
        }

        [TestMethod]
        public void TestActivityWithMultiStructuredSuggestionActions()
        {
            dynamic data = new JObject();
            data.text = "textContent";
            var lgResult = GetLGFile().EvaluateTemplate("activityWithMultiStructuredSuggestionActions", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertActivityWithMultiStructuredSuggestionActions(activity);
        }

        [TestMethod]
        public void TestActivityWithMultiStringSuggestionActions()
        {
            dynamic data = new JObject();
            data.text = "textContent";
            var lgResult = GetLGFile().EvaluateTemplate("activityWithMultiStringSuggestionActions", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertActivityWithMultiStringSuggestionActions(activity);
        }

        [TestMethod]
        public void TestHeroCardTemplate()
        {
            dynamic data = new JObject();
            data.type = "herocard";
            var lgResult = GetLGFile().EvaluateTemplate("HeroCardTemplate", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertHeroCardActivity(activity);
        }

        [TestMethod]
        public void TestThumbnailCardTemplate()
        {
            dynamic data = new JObject();
            data.type = "thumbnailcard";
            var lgResult = GetLGFile().EvaluateTemplate("ThumbnailCardTemplate", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertThumbnailCardActivity(activity);
        }

        [TestMethod]
        public void TestAudioCardTemplate()
        {
            dynamic data = new JObject();
            data.type = "audiocard";
            var lgResult = GetLGFile().EvaluateTemplate("AudioCardTemplate", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertAudioCardActivity(activity);
        }

        [TestMethod]
        public void TestVideoCardTemplate()
        {
            dynamic data = new JObject();
            data.type = "videocard";
            var lgResult = GetLGFile().EvaluateTemplate("VideoCardTemplate", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertVideoCardActivity(activity);
        }

        [TestMethod]
        public void TestSigninCardTemplate()
        {
            dynamic data = new JObject();
            data.signinlabel = "Sign in";
            data.url = "https://login.microsoftonline.com/";
            var lgResult = GetLGFile().EvaluateTemplate("SigninCardTemplate", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertSigninCardActivity(activity);
        }

        [TestMethod]
        public void TestOAuthCardTemplate()
        {
            dynamic data = new JObject();
            data.signinlabel = "Sign in";
            data.url = "https://login.microsoftonline.com/";
            data.connectionName = "MyConnection";
            var lgResult = GetLGFile().EvaluateTemplate("OAuthCardTemplate", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertOAuthCardActivity(activity);
        }

        [TestMethod]
        public void TestReceiptCardTemplate()
        {
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
            var lgResult = GetLGFile().EvaluateTemplate("ReceiptCardTemplate", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
            AssertReceiptCardActivity(activity);
        }

        [TestMethod]
        public void TestSuggestedActionsReference()
        {
            dynamic data = new JObject();
            data.text = "textContent";
            var lgResult = GetLGFile().EvaluateTemplate("SuggestedActionsReference", data);
            var activity = ActivityFactory.CreateActivity(lgResult);
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

        private LGFile GetLGFile()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "lg", "NormalStructuredLG.lg");
            return LGParser.ParseFile(path);
        }
    }
}
