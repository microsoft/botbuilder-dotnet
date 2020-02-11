// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [TestClass]
    public class RegexRecognizerTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(RegexRecognizerTests)), monitorChanges: false);
        }

        [TestMethod]
        public async Task RegexRecognizerTests_Entities()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task RegexRecognizerTests_Intents()
        {
            var recognizer = new RegexRecognizer()
            {
                Intents = new List<IntentPattern>()
                {
                     new IntentPattern("codeIntent", "(?<code>[a-z][0-9])"),
                     new IntentPattern("colorIntent", "(?i)(color|colour)"),
                },
                Entities = new EntityRecognizerSet()
                {
                    new AgeEntityRecognizer(),
                    new ConfirmationEntityRecognizer(),
                    new CurrencyEntityRecognizer(),
                    new DateTimeEntityRecognizer(),
                    new DimensionEntityRecognizer(),
                    new EmailEntityRecognizer(),
                    new GuidEntityRecognizer(),
                    new HashtagEntityRecognizer(),
                    new IpEntityRecognizer(),
                    new MentionEntityRecognizer(),
                    new NumberEntityRecognizer(),
                    new NumberRangeEntityRecognizer(),
                    new OrdinalEntityRecognizer(),
                    new PercentageEntityRecognizer(),
                    new PhoneNumberEntityRecognizer(),
                    new TemperatureEntityRecognizer(),
                    new UrlEntityRecognizer(),
                    new RegexEntityRecognizer() { Name = "color", Pattern = "(?i)(red|green|blue|purple|orange|violet|white|black)" },
                    new RegexEntityRecognizer() { Name = "backgroundColor", Pattern = "(?i)(back|background) {color}" },
                    new RegexEntityRecognizer() { Name = "foregroundColor", Pattern = "(?i)(foreground|front) {color}" },
                }
            };

            // test with DC
            var dc = CreateContext("intent a1 b2");
            var result = await recognizer.RecognizeAsync(dc, CancellationToken.None);
            ValidateCodeIntent(result);

            dc = CreateContext("I would like color red and orange");
            result = await recognizer.RecognizeAsync(dc, CancellationToken.None);
            ValidateColorIntent(result);

            dc = CreateContext(string.Empty);

            // test custom activity
            var activity = Activity.CreateMessageActivity();
            activity.Text = "intent a1 b2";
            activity.Locale = Culture.English;
            result = await recognizer.RecognizeAsync(dc,  (Activity)activity, CancellationToken.None);
            ValidateCodeIntent(result);

            activity.Text = "I would like color red and orange";
            result = await recognizer.RecognizeAsync(dc, (Activity)activity, CancellationToken.None);
            ValidateColorIntent(result);

            // test text, locale
            result = await recognizer.RecognizeAsync(dc, "intent a1 b2", Culture.English, CancellationToken.None);
            ValidateCodeIntent(result);

            result = await recognizer.RecognizeAsync(dc, "I would like color red and orange", Culture.English, CancellationToken.None);
            ValidateColorIntent(result);
        }

        private static void ValidateColorIntent(RecognizerResult result)
        {
            Assert.AreEqual(1, result.Intents.Count, "Should recognize one intent");
            Assert.AreEqual("colorIntent", result.Intents.Select(i => i.Key).First(), "Should recognize colorIntent");

            // entity assertions from capture group
            dynamic entities = result.Entities;
            Assert.IsNotNull(entities.color, "should find color");
            Assert.IsNull(entities.code, "should not find code");
            Assert.AreEqual(2, entities.color.Count, "should find 2 colors");
            Assert.AreEqual("red", (string)entities.color[0], "should find red");
            Assert.AreEqual("orange", (string)entities.color[1], "should find orange");
        }

        private static void ValidateCodeIntent(RecognizerResult result)
        {
            // intent assertions
            Assert.AreEqual(1, result.Intents.Count, "Should recognize one intent");
            Assert.AreEqual("codeIntent", result.Intents.Select(i => i.Key).First(), "Should recognize codeIntent");

            // entity assertions from capture group
            dynamic entities = result.Entities;
            Assert.IsNotNull(entities.code, "should find code");
            Assert.IsNull(entities.color, "should not find color");
            Assert.AreEqual(2, entities.code.Count, "should find 2 codes");
            Assert.AreEqual("a1", (string)entities.code[0], "should find a1");
            Assert.AreEqual("b2", (string)entities.code[1], "should find b2");
        }

        private static DialogContext CreateContext(string text)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = text;
            return new DialogContext(new DialogSet(), new TurnContext(new TestAdapter(), (Activity)activity), new DialogState());
        }
    }
}
