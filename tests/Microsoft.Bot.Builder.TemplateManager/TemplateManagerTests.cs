// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.TemplateManager.Tests
{
    [TestClass]
    [TestCategory("Template")]
    public class TemplateManagerTests
    {
        private static TestContext _testContext;
        private static LanguageTemplateDictionary templates1;
        private static LanguageTemplateDictionary templates2;

        public TestContext TestContext { get; set; }

        [AssemblyInitialize]
        public static void SetupDictionaries(TestContext testContext)
        {
            _testContext = testContext;
            templates1 = new LanguageTemplateDictionary
            {
                ["default"] = new TemplateIdMap
                {
                    { "stringTemplate", (context, data) => $"default: {data.name}" },
                    { "activityTemplate", (context, data) => { return new Activity() { Type = ActivityTypes.Message, Text = $"(Activity)default: {data.name}" }; } },
                    { "stringTemplate2", (context, data) => $"default: Yo {data.name}" },
                },
                ["en"] = new TemplateIdMap
                {
                    { "stringTemplate", (context, data) => $"en: {data.name}" },
                    { "activityTemplate", (context, data) => { return new Activity() { Type = ActivityTypes.Message, Text = $"(Activity)en: {data.name}" }; } },
                    { "stringTemplate2", (context, data) => $"en: Yo {data.name}" },
                },
                ["fr"] = new TemplateIdMap
                {
                    { "stringTemplate", (context, data) => $"fr: {data.name}" },
                    { "activityTemplate", (context, data) => { return new Activity() { Type = ActivityTypes.Message, Text = $"(Activity)fr: {data.name}" }; } },
                    { "stringTemplate2", (context, data) => $"fr: Yo {data.name}" },
                },
            };
            templates2 = new LanguageTemplateDictionary
            {
                ["en"] = new TemplateIdMap
                {
                    { "stringTemplate2", (context, data) => $"en: StringTemplate2 override {data.name}" },
                },
            };
        }

        [TestMethod]
        public void TemplateManager_Registration()
        {
            var templateManager = new TemplateManager();
            Assert.AreEqual(templateManager.List().Count, 0, "nothing registered yet");
            var templateEngine1 = new DictionaryRenderer(templates1);
            var templateEngine2 = new DictionaryRenderer(templates2);
            templateManager.Register(templateEngine1);
            Assert.AreEqual(templateManager.List().Count, 1, "one registered");

            templateManager.Register(templateEngine1);
            Assert.AreEqual(templateManager.List().Count, 1, "only  one registered");

            templateManager.Register(templateEngine2);
            Assert.AreEqual(templateManager.List().Count, 2, "two registered");
        }

        [TestMethod]
        public void TemplateManager_MultiTemplate()
        {
            var templateManager = new TemplateManager();
            Assert.AreEqual(templateManager.List().Count, 0, "nothing registered yet");
            var templateEngine1 = new DictionaryRenderer(templates1);
            var templateEngine2 = new DictionaryRenderer(templates2);
            templateManager.Register(templateEngine1);
            Assert.AreEqual(templateManager.List().Count, 1, "one registered");

            templateManager.Register(templateEngine1);
            Assert.AreEqual(templateManager.List().Count, 1, "only  one registered");

            templateManager.Register(templateEngine2);
            Assert.AreEqual(templateManager.List().Count, 2, "two registered");
        }

        [TestMethod]
        public async Task DictionaryTemplateEngine_SimpleStringBinding()
        {
            var engine = new DictionaryRenderer(templates1);
            var result = await engine.RenderTemplate(null, "en", "stringTemplate", new { name = "joe" });
            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreEqual("en: joe", (string)result);
        }

        [TestMethod]
        public async Task DictionaryTemplateEngine_SimpleActivityBinding()
        {
            var engine = new DictionaryRenderer(templates1);
            var result = await engine.RenderTemplate(null, "en", "activityTemplate", new { name = "joe" });
            Assert.IsInstanceOfType(result, typeof(Activity));
            var activity = result as Activity;
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("(Activity)en: joe", activity.Text);
        }

        [TestMethod]
        public async Task TemplateManager_defaultlookup()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var templateManager = new TemplateManager()
                .Register(new DictionaryRenderer(templates1))
                .Register(new DictionaryRenderer(templates2));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    var templateId = context.Activity.AsMessageActivity().Text.Trim();
                    await templateManager.ReplyWith(context, templateId, new { name = "joe" });
                })
                .Send("stringTemplate").AssertReply("default: joe")
                .Send("activityTemplate").AssertReply("(Activity)default: joe")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TemplateManager_DataDefined()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var templateManager = new TemplateManager()
            {
                Renderers =
                {
                    new DictionaryRenderer(templates1),
                    new DictionaryRenderer(templates2)
                }
            };

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                var templateId = context.Activity.AsMessageActivity().Text.Trim();
                await templateManager.ReplyWith(context, templateId, new { name = "joe" });
            })
                .Send("stringTemplate").AssertReply("default: joe")
                .Send("activityTemplate").AssertReply("(Activity)default: joe")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TemplateManager_enLookup()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var templateManager = new TemplateManager()
                .Register(new DictionaryRenderer(templates1))
                .Register(new DictionaryRenderer(templates2));

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                context.Activity.AsMessageActivity().Locale = "en"; // force to english
                var templateId = context.Activity.AsMessageActivity().Text.Trim();
                await templateManager.ReplyWith(context, templateId, new { name = "joe" });
            })
                .Send("stringTemplate").AssertReply("en: joe")
                .Send("activityTemplate").AssertReply("(Activity)en: joe")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TemplateManager_frLookup()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var templateManager = new TemplateManager()
                .Register(new DictionaryRenderer(templates1))
                .Register(new DictionaryRenderer(templates2));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    context.Activity.AsMessageActivity().Locale = "fr"; // force to french
                    var templateId = context.Activity.AsMessageActivity().Text.Trim();
                    await templateManager.ReplyWith(context, templateId, new { name = "joe" });
                })
                .Send("stringTemplate").AssertReply("fr: joe")
                .Send("activityTemplate").AssertReply("(Activity)fr: joe")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TemplateManager_override()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var templateManager = new TemplateManager()
                .Register(new DictionaryRenderer(templates1))
                .Register(new DictionaryRenderer(templates2));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    context.Activity.AsMessageActivity().Locale = "fr"; // force to french
                    var templateId = context.Activity.AsMessageActivity().Text.Trim();
                    await templateManager.ReplyWith(context, templateId, new { name = "joe" });
                })
                .Send("stringTemplate2").AssertReply("fr: Yo joe")
                .Send("activityTemplate").AssertReply("(Activity)fr: joe")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TemplateManager_useTemplateEngine()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var templateManager = new TemplateManager()
                .Register(new DictionaryRenderer(templates1))
                .Register(new DictionaryRenderer(templates2));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    var templateId = context.Activity.AsMessageActivity().Text.Trim();
                    await templateManager.ReplyWith(context, templateId, new { name = "joe" });
                })
                .Send("stringTemplate").AssertReply("default: joe")
                .Send("activityTemplate").AssertReply("(Activity)default: joe")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TemplateManagerMiddleware_Declarative()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()))
                                .Use(new TemplateManagerMiddleware()
                                {
                                    Renderers =
                                    {
                                        new DictionaryRenderer(templates1),
                                        new DictionaryRenderer(templates2)
                                    }
                                });

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                var templateId = context.Activity.AsMessageActivity().Text.Trim();
                var templateActivity = TemplateManager.CreateTemplateActivity(templateId, new { name = "joe" });
                await context.SendActivityAsync(templateActivity);
            })
                .Send("stringTemplate")
                    .AssertReply("default: joe")
                .Send("activityTemplate")
                    .AssertReply("(Activity)default: joe")
                .StartTestAsync();
        }
    }
}
