using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class ActivityFactoryTests
    {
        public ActivityFactoryTests()
        {
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new AdaptiveTestingComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
        }

        [Fact]
        public void TestInlineActivityFactory()
        {
            var lgResult = GetNormalStructureLGFile().EvaluateText("text").ToString();
            var activity = ActivityFactory.FromObject(lgResult);

            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal("text", activity.Text);
            Assert.Equal("text", activity.Speak);
            Assert.Null(activity.InputHint);

            var data = new JObject();
            data["title"] = "titleContent";
            data["text"] = "textContent";
            var cardActionLgResult = GetNormalStructureLGFile().EvaluateText("${HerocardWithCardAction()}", data);
            activity = ActivityFactory.FromObject(cardActionLgResult);
            AssertCardActionActivity(activity);
        }

        [Fact]
        public void TestNotSupportStructuredType()
        {
            // fallback to text activity
            var lgResult = GetNormalStructureLGFile().Evaluate("notSupport");
            var activity = ActivityFactory.FromObject(lgResult);
            Assert.Equal(0, activity.Attachments.Count);
            Assert.Equal("{\"lgType\":\"Acti\",\"key\":\"value\"}", activity.Text.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty));
        }

        [Fact]
        public void TestHerocardWithCardAction()
        {
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgResult = GetNormalStructureLGFile().Evaluate("HerocardWithCardAction", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertCardActionActivity(activity);
        }

        [Fact]
        public void TestAdaptivecardActivity()
        {
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            var lgResult = GetNormalStructureLGFile().Evaluate("adaptivecardActivity", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertAdaptiveCardActivity(activity);
        }

        [Fact]
        public void TestExternalAdaptiveCardActivity()
        {
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            var lgResult = GetNormalStructureLGFile().Evaluate("externalAdaptiveCardActivity", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertAdaptiveCardActivity(activity);
        }

        [Fact]
        public void TestMultiExternalAdaptiveCardActivity()
        {
            dynamic data = new JObject();
            data.titles = new JArray() { "test0", "test1", "test2" };
            var lgResult = GetNormalStructureLGFile().Evaluate("multiExternalAdaptiveCardActivity", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertMultiAdaptiveCardActivity(activity);
        }

        [Fact]
        public void TestAdaptivecardActivityWithAttachmentStructure()
        {
            dynamic data = new JObject();
            data.adaptiveCardTitle = "test";
            var lgResult = GetNormalStructureLGFile().Evaluate("adaptivecardActivityWithAttachmentStructure", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertAdaptiveCardActivity(activity);
        }

        [Fact]
        public void TestExternalHeroCardActivity()
        {
            dynamic data = new JObject();
            data.type = "imBack";
            data.title = "taptitle";
            data.value = "tapvalue";
            var lgResult = GetNormalStructureLGFile().Evaluate("externalHeroCardActivity", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertActivityWithHeroCardAttachment(activity);
        }

        [Fact]
        public void TestEventActivity()
        {
            dynamic data = new JObject();
            data.text = "textContent";
            var lgResult = GetNormalStructureLGFile().Evaluate("eventActivity", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertEventActivity(activity);
        }

        [Fact]
        public void TestHandoffActivity()
        {
            dynamic data = new JObject();
            data.text = "textContent";
            var lgResult = GetNormalStructureLGFile().Evaluate("handoffActivity", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertHandoffActivity(activity);
        }

        [Fact]
        public void TestActivityWithHeroCardAttachment()
        {
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgResult = GetNormalStructureLGFile().Evaluate("activityWithHeroCardAttachment", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertActivityWithHeroCardAttachment(activity);
        }

        [Fact]
        public void TestHerocardAttachment()
        {
            dynamic data = new JObject();
            data.type = "imBack";
            data.title = "taptitle";
            data.value = "tapvalue";
            var lgResult = GetNormalStructureLGFile().Evaluate("herocardAttachment", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertActivityWithHeroCardAttachment(activity);
        }

        [Fact]
        public void TestHerocardActivityWithAttachmentStructure()
        {
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgResult = GetNormalStructureLGFile().Evaluate("activityWithMultiAttachments", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertActivityWithMultiAttachments(activity);
        }

        [Fact]
        public void TestActivityWithSuggestionActions()
        {
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgResult = GetNormalStructureLGFile().Evaluate("activityWithSuggestionActions", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertActivityWithSuggestionActions(activity);
        }

        [Fact]
        public void TestMessageActivityAll()
        {
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";
            var lgResult = GetNormalStructureLGFile().Evaluate("messageActivityAll", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertMessageActivityAll(activity);
        }

        [Fact]
        public void TestActivityWithMultiStructuredSuggestionActions()
        {
            dynamic data = new JObject();
            data.text = "textContent";
            var lgResult = GetNormalStructureLGFile().Evaluate("activityWithMultiStructuredSuggestionActions", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertActivityWithMultiStructuredSuggestionActions(activity);
        }

        [Fact]
        public void TestActivityWithMultiStringSuggestionActions()
        {
            dynamic data = new JObject();
            data.text = "textContent";
            var lgResult = GetNormalStructureLGFile().Evaluate("activityWithMultiStringSuggestionActions", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertActivityWithMultiStringSuggestionActions(activity);
        }

        [Fact]
        public void TestHeroCardTemplate()
        {
            dynamic data = new JObject();
            data.type = "herocard";
            var lgResult = GetNormalStructureLGFile().Evaluate("HeroCardTemplate", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertHeroCardActivity(activity);
        }

        [Fact]
        public void TestThumbnailCardTemplate()
        {
            dynamic data = new JObject();
            data.type = "thumbnailcard";
            var lgResult = GetNormalStructureLGFile().Evaluate("ThumbnailCardTemplate", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertThumbnailCardActivity(activity);
        }

        [Fact]
        public void TestAudioCardTemplate()
        {
            dynamic data = new JObject();
            data.type = "audiocard";
            var lgResult = GetNormalStructureLGFile().Evaluate("AudioCardTemplate", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertAudioCardActivity(activity);
        }

        [Fact]
        public void TestVideoCardTemplate()
        {
            dynamic data = new JObject();
            data.type = "videocard";
            var lgResult = GetNormalStructureLGFile().Evaluate("VideoCardTemplate", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertVideoCardActivity(activity);
        }

        [Fact]
        public void TestSigninCardTemplate()
        {
            dynamic data = new JObject();
            data.signinlabel = "Sign in";
            data.url = "https://login.microsoftonline.com/";
            var lgResult = GetNormalStructureLGFile().Evaluate("SigninCardTemplate", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertSigninCardActivity(activity);
        }

        [Fact]
        public void TestOAuthCardTemplate()
        {
            dynamic data = new JObject();
            data.signinlabel = "Sign in";
            data.url = "https://login.microsoftonline.com/";
            data.connectionName = "MyConnection";
            var lgResult = GetNormalStructureLGFile().Evaluate("OAuthCardTemplate", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertOAuthCardActivity(activity);
        }

        [Fact]
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
            var lgResult = GetNormalStructureLGFile().Evaluate("ReceiptCardTemplate", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertReceiptCardActivity(activity);
        }

        [Fact]
        public void TestSuggestedActionsReference()
        {
            dynamic data = new JObject();
            data.text = "textContent";
            var lgResult = GetNormalStructureLGFile().Evaluate("SuggestedActionsReference", data);
            var activity = ActivityFactory.FromObject(lgResult);
            AssertSuggestedActionsReferenceActivity(activity);
        }

        [Fact]
        public void CheckOutPutNotFromStructuredLG()
        {
            var diagnostics = ActivityFactory.CheckLGResult("Not a valid json");
            Assert.Equal(diagnostics.Count, 1);
            Assert.Equal("[WARNING]LG output is not a json object, and will fallback to string format.", diagnostics[0]);
        }

        [Fact]
        public void CheckStructuredLGDiagnostics()
        {
            var lgResult = GetDiagnosticStructureLGFile().Evaluate("ErrorStructuredType", null);
            var diagnostics = ActivityFactory.CheckLGResult(lgResult);
            Assert.Equal(diagnostics.Count, 1);
            Assert.Equal("[WARNING]Type 'mystruct' is not supported currently.", diagnostics[0]);

            lgResult = GetDiagnosticStructureLGFile().Evaluate("ErrorActivityType", null);
            
            diagnostics = ActivityFactory.CheckLGResult(lgResult);
            Assert.Equal(2, diagnostics.Count);
            Assert.Equal("[ERROR]'xxx' is not a valid activity type.", diagnostics[0]);
            Assert.Equal("[WARNING]'invalidproperty' not support in Activity.", diagnostics[1]);

            lgResult = GetDiagnosticStructureLGFile().Evaluate("ErrorMessage", null);
            diagnostics = ActivityFactory.CheckLGResult(lgResult);
            Assert.Equal(diagnostics.Count, 5);
            Assert.Equal("[WARNING]'attachment,suggestedaction' not support in Activity.", diagnostics[0]);
            Assert.Equal("[WARNING]'mystruct' is not card action type.", diagnostics[1]);
            Assert.Equal("[ERROR]'yyy' is not a valid card action type.", diagnostics[2]);
            Assert.Equal("[ERROR]'notsure' is not a boolean value.", diagnostics[3]);
            Assert.Equal("[WARNING]'mystruct' is not an attachment type.", diagnostics[4]);
        }

        private static string GetProjectFolder()
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
        }

        private void AssertSuggestedActionsReferenceActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal("textContent", activity.Text);
           
            Assert.Equal(5, activity.SuggestedActions.Actions.Count);
            Assert.Equal("Add todo", activity.SuggestedActions.Actions[0].Value);
            Assert.Equal("View Todo", activity.SuggestedActions.Actions[1].Value);
            Assert.Equal("Remove Todo", activity.SuggestedActions.Actions[2].Value);
            Assert.Equal("Cancel", activity.SuggestedActions.Actions[3].Value);
            Assert.Equal("Help", activity.SuggestedActions.Actions[4].Value);
        }

        private void AssertMessageActivityAll(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal("textContent", activity.Text);
            Assert.Equal("textContent", activity.Speak);
            Assert.Equal("accepting", activity.InputHint);
            var semanticAction = activity.SemanticAction;
            Assert.Equal("actionId", semanticAction.Id);
            Assert.Equal(1, semanticAction.Entities.Count);
            Assert.Equal(true, semanticAction.Entities.ContainsKey("key1"));
            Assert.Equal("entityType", semanticAction.Entities["key1"].Type);

            Assert.Equal(1, activity.Attachments.Count);
            Assert.Equal(AttachmentLayoutTypes.List, activity.AttachmentLayout);
            Assert.Equal(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.NotNull(card);
            var tap = card.Tap;
            Assert.Equal("taptitle", tap.Title);
            Assert.Equal("tapvalue", tap.Value);
            Assert.Equal("imBack", tap.Type);
            Assert.Equal("textContent", card.Text);
            Assert.Equal(1, card.Buttons.Count);
            Assert.Equal($"imBack", card.Buttons[0].Type);
            Assert.Equal($"titleContent", card.Buttons[0].Title);
            Assert.Equal($"textContent", card.Buttons[0].Value);
            Assert.Equal(2, activity.SuggestedActions.Actions.Count);
            Assert.Equal("firstItem", activity.SuggestedActions.Actions[0].Title);
            Assert.Equal("firstItem", activity.SuggestedActions.Actions[0].Value);
            Assert.Equal("titleContent", activity.SuggestedActions.Actions[1].Title);
            Assert.Equal("textContent", activity.SuggestedActions.Actions[1].Value);
        }

        private void AssertActivityWithSuggestionActions(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal("textContent", activity.Text);
            Assert.Equal(2, activity.SuggestedActions.Actions.Count);
            Assert.Equal("firstItem", activity.SuggestedActions.Actions[0].Title);
            Assert.Equal("firstItem", activity.SuggestedActions.Actions[0].Value);
            Assert.Equal("titleContent", activity.SuggestedActions.Actions[1].Title);
            Assert.Equal("textContent", activity.SuggestedActions.Actions[1].Value);
        }

        private void AssertActivityWithMultiAttachments(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.True(string.IsNullOrEmpty(activity.Text));
            Assert.True(string.IsNullOrEmpty(activity.Speak));
            Assert.Equal(2, activity.Attachments.Count);
            Assert.Equal(ThumbnailCard.ContentType, activity.Attachments[1].ContentType);
            var card = ((JObject)activity.Attachments[1].Content).ToObject<ThumbnailCard>();
            Assert.NotNull(card);
            Assert.Equal("Cheese gromit!", card.Title);
            Assert.Equal("type", card.Subtitle);
            Assert.Equal("This is some text describing the card, it's cool because it's cool", card.Text);
            Assert.Equal("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url);
            Assert.Equal("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[1].Url);
            Assert.Equal(3, card.Buttons.Count);
            for (int i = 0; i <= 2; i++)
            {
                Assert.Equal($"Option {i + 1}", card.Buttons[i].Title);
            }
        }

        private void AssertActivityWithHeroCardAttachment(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.True(string.IsNullOrEmpty(activity.Text));
            Assert.True(string.IsNullOrEmpty(activity.Speak));
            Assert.Equal(1, activity.Attachments.Count);
            Assert.Equal(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            var tap = card.Tap;
            Assert.Equal("taptitle", tap.Title);
            Assert.Equal("tapvalue", tap.Value);
            Assert.Equal("imBack", tap.Type);
            Assert.NotNull(card);
            Assert.Equal("titleContent", card.Title);
            Assert.Equal("textContent", card.Text);
            Assert.Equal(1, card.Buttons.Count);
            Assert.Equal($"imBack", card.Buttons[0].Type);
            Assert.Equal($"titleContent", card.Buttons[0].Title);
            Assert.Equal($"textContent", card.Buttons[0].Value);
        }

        private void AssertHandoffActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Handoff, activity.Type);
            Assert.Equal("textContent", activity.Name);
            Assert.Equal("textContent", activity.Value);
        }

        private void AssertEventActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal("textContent", activity.Name);
            Assert.Equal("textContent", activity.Value);
        }

        private void AssertAdaptiveCardActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.True(string.IsNullOrEmpty(activity.Text));
            Assert.True(string.IsNullOrEmpty(activity.Speak));
            Assert.Equal(1, activity.Attachments.Count);
            Assert.Equal("application/vnd.microsoft.card.adaptive", activity.Attachments[0].ContentType);
            Assert.Equal("test", (string)((dynamic)activity.Attachments[0].Content).body[0].text);
        }

        private void AssertMultiAdaptiveCardActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.True(string.IsNullOrEmpty(activity.Text));
            Assert.True(string.IsNullOrEmpty(activity.Speak));
            Assert.Equal(3, activity.Attachments.Count);
            for (int i = 0; i < 3; ++i)
            {
                Assert.Equal("application/vnd.microsoft.card.adaptive", activity.Attachments[i].ContentType);
                Assert.Equal($"test{i}", (string)((dynamic)activity.Attachments[i].Content).body[0].text);
            }
        }

        private void AssertCardActionActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.True(string.IsNullOrEmpty(activity.Text));
            Assert.True(string.IsNullOrEmpty(activity.Speak));
            Assert.Equal(1, activity.Attachments.Count);
            Assert.Equal(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.NotNull(card);
            Assert.Equal("titleContent", card.Title);
            Assert.Equal("textContent", card.Text);
            Assert.Equal(1, card.Buttons.Count);
            Assert.Equal($"imBack", card.Buttons[0].Type);
            Assert.Equal($"titleContent", card.Buttons[0].Title);
            Assert.Equal($"textContent", card.Buttons[0].Value);
        }

        private void AssertThumbnailCardActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.True(string.IsNullOrEmpty(activity.Text));
            Assert.True(string.IsNullOrEmpty(activity.Speak));
            Assert.Equal(1, activity.Attachments.Count);
            Assert.Equal(ThumbnailCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<ThumbnailCard>();
            Assert.NotNull(card);
            Assert.Equal("Cheese gromit!", card.Title);
            Assert.Equal("thumbnailcard", card.Subtitle);
            Assert.Equal("This is some text describing the card, it's cool because it's cool", card.Text);
            Assert.Equal("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url);
            Assert.Equal("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[1].Url);
            Assert.Equal(3, card.Buttons.Count);
            for (int i = 0; i <= 2; i++)
            {
                Assert.Equal($"Option {i + 1}", card.Buttons[i].Title);
            }
        }

        private void AssertHeroCardActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.True(string.IsNullOrEmpty(activity.Text));
            Assert.True(string.IsNullOrEmpty(activity.Speak));
            Assert.Equal(1, activity.Attachments.Count);
            Assert.Equal(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.NotNull(card);
            Assert.Equal("Cheese gromit!", card.Title);
            Assert.Equal("herocard", card.Subtitle);
            Assert.Equal("This is some text describing the card, it's cool because it's cool", card.Text);
            Assert.Equal("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url);
            Assert.Equal("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[1].Url);
            Assert.Equal(3, card.Buttons.Count);
            for (int i = 0; i <= 2; i++)
            {
                Assert.Equal($"Option {i + 1}", card.Buttons[i].Title);
            }
        }

        private void AssertAudioCardActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.True(string.IsNullOrEmpty(activity.Text));
            Assert.True(string.IsNullOrEmpty(activity.Speak));
            Assert.Equal(1, activity.Attachments.Count);
            Assert.Equal(AudioCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<AudioCard>();
            Assert.NotNull(card);
            Assert.Equal("Cheese gromit!", card.Title);
            Assert.Equal("audiocard", card.Subtitle);
            Assert.Equal("This is some text describing the card, it's cool because it's cool", card.Text);
            Assert.Equal("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Image.Url);
            Assert.Equal("https://contoso.com/media/AllegrofromDuetinCMajor.mp3", card.Media[0].Url);
            Assert.Equal(false, card.Shareable);
            Assert.Equal(true, card.Autoloop);
            Assert.Equal(true, card.Autostart);
            Assert.Equal("16:9", card.Aspect);
            Assert.Equal(3, card.Buttons.Count);
            for (int i = 0; i <= 2; i++)
            {
                Assert.Equal($"Option {i + 1}", card.Buttons[i].Title);
            }
        }

        private void AssertVideoCardActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.True(string.IsNullOrEmpty(activity.Text));
            Assert.True(string.IsNullOrEmpty(activity.Speak));
            Assert.Equal(1, activity.Attachments.Count);
            Assert.Equal(VideoCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<VideoCard>();
            Assert.NotNull(card);
            Assert.Equal("Cheese gromit!", card.Title);
            Assert.Equal("videocard", card.Subtitle);
            Assert.Equal("This is some text describing the card, it's cool because it's cool", card.Text);
            Assert.Equal("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Image.Url);
            Assert.Equal("https://youtu.be/530FEFogfBQ", card.Media[0].Url);
            Assert.Equal(false, card.Shareable);
            Assert.Equal(true, card.Autoloop);
            Assert.Equal(true, card.Autostart);
            Assert.Equal("16:9", card.Aspect);
            Assert.Equal(3, card.Buttons.Count);
            for (int i = 0; i <= 2; i++)
            {
                Assert.Equal($"Option {i + 1}", card.Buttons[i].Title);
            }
        }

        private void AssertSigninCardActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.True(string.IsNullOrEmpty(activity.Text));
            Assert.True(string.IsNullOrEmpty(activity.Speak));
            Assert.Equal(1, activity.Attachments.Count);
            Assert.Equal(SigninCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<SigninCard>();
            Assert.NotNull(card);
            Assert.Equal("This is some text describing the card, it's cool because it's cool", card.Text);
            Assert.Equal(1, card.Buttons.Count);
            Assert.Equal($"Sign in", card.Buttons[0].Title);
            Assert.Equal(ActionTypes.Signin, card.Buttons[0].Type);
            Assert.Equal($"https://login.microsoftonline.com/", card.Buttons[0].Value);
        }

        private void AssertOAuthCardActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.True(string.IsNullOrEmpty(activity.Text));
            Assert.True(string.IsNullOrEmpty(activity.Speak));
            Assert.Equal(1, activity.Attachments.Count);
            Assert.Equal(OAuthCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<OAuthCard>();
            Assert.NotNull(card);
            Assert.Equal("This is some text describing the card, it's cool because it's cool", card.Text);
            Assert.Equal("MyConnection", card.ConnectionName);
            Assert.Equal(1, card.Buttons.Count);
            Assert.Equal($"Sign in", card.Buttons[0].Title);
            Assert.Equal(ActionTypes.Signin, card.Buttons[0].Type);
            Assert.Equal($"https://login.microsoftonline.com/", card.Buttons[0].Value);
        }

        private void AssertActivityWithMultiStructuredSuggestionActions(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal("textContent", activity.Text);
            Assert.Equal(3, activity.SuggestedActions.Actions.Count);
            Assert.Equal("first suggestion", activity.SuggestedActions.Actions[0].Value);
            Assert.Equal("first suggestion", activity.SuggestedActions.Actions[0].Title);
            Assert.Equal("first suggestion", activity.SuggestedActions.Actions[0].Text);
            Assert.Equal("second suggestion", activity.SuggestedActions.Actions[1].Value);
            Assert.Equal("second suggestion", activity.SuggestedActions.Actions[1].Title);
            Assert.Equal("second suggestion", activity.SuggestedActions.Actions[1].Text);
            Assert.Equal("third suggestion", activity.SuggestedActions.Actions[2].Value);
            Assert.Equal("third suggestion", activity.SuggestedActions.Actions[2].Title);
            Assert.Equal("third suggestion", activity.SuggestedActions.Actions[2].Text);
        }

        private void AssertActivityWithMultiStringSuggestionActions(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal("textContent", activity.Text);
            Assert.Equal(3, activity.SuggestedActions.Actions.Count);
            Assert.Equal("first suggestion", activity.SuggestedActions.Actions[0].Value);
            Assert.Equal("first suggestion", activity.SuggestedActions.Actions[0].Title);
            Assert.Equal("second suggestion", activity.SuggestedActions.Actions[1].Value);
            Assert.Equal("second suggestion", activity.SuggestedActions.Actions[1].Title);
            Assert.Equal("third suggestion", activity.SuggestedActions.Actions[2].Value);
            Assert.Equal("third suggestion", activity.SuggestedActions.Actions[2].Title);
        }

        private void AssertReceiptCardActivity(Activity activity)
        {
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal(1, activity.Attachments.Count);
            Assert.Equal(ReceiptCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<ReceiptCard>();
            Assert.NotNull(card);
            Assert.Equal("John Doe", card.Title);
            Assert.Equal("$ 7.50", card.Tax);
            Assert.Equal("$ 90.95", card.Total);
            var buttons = card.Buttons;

            Assert.Equal(1, buttons.Count);
            Assert.Equal(ActionTypes.OpenUrl, buttons[0].Type);
            Assert.Equal("More information", buttons[0].Title);
            Assert.Equal("https://account.windowsazure.com/content/6.10.1.38-.8225.160809-1618/aux-pre/images/offer-icon-freetrial.png", buttons[0].Image);
            Assert.Equal("https://azure.microsoft.com/en-us/pricing/", buttons[0].Value);

            var facts = card.Facts;
            Assert.Equal(2, facts.Count);
            Assert.Equal("Order Number", facts[0].Key);
            Assert.Equal("1234", facts[0].Value);
            Assert.Equal("Payment Method", facts[1].Key);
            Assert.Equal("VISA 5555-****", facts[1].Value);

            var items = card.Items;
            Assert.Equal(2, items.Count);
            Assert.Equal("Data Transfer", items[0].Title);
            Assert.Equal("https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png", items[0].Image.Url);
            Assert.Equal("$ 38.45", items[0].Price);
            Assert.Equal("368", items[0].Quantity);
            Assert.Equal("App Service", items[1].Title);
            Assert.Equal("https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png", items[1].Image.Url);
            Assert.Equal("$ 45.00", items[1].Price);
            Assert.Equal("720", items[1].Quantity);
        }

        private Templates GetNormalStructureLGFile()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "lg", "NormalStructuredLG.lg");
            return Templates.ParseFile(path);
        }

        private Templates GetDiagnosticStructureLGFile()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "lg", "DignosticStructuredLG.lg");
            return Templates.ParseFile(path);
        }
    }
}
