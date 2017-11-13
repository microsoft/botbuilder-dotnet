using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Templates;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Template")]
    public class Template_TemplateManager_Register
    {
        private static TestContext _testContext;
        private static TemplateDictionary templates1;
        private static TemplateDictionary templates2;

        [AssemblyInitialize]
        public static void SetupDictionaries(TestContext testContext)
        {
            _testContext = testContext;
            templates1 = new TemplateDictionary
            {
                ["default"] = new TemplateIdMap
                {
                    { "stringTemplate", (context, data) => $"default: { data.name}" },
                    { "activityTemplate", (context, data) => { return new Activity() { Type = ActivityTypes.Message, Text = $"(Activity)default: { data.name}" }; } },
                    { "stringTemplate2", (context, data) => $"default: Yo { data.name}" }
                },
                ["en"] = new TemplateIdMap
                {
                    { "stringTemplate", (context, data) => $"en: { data.name}" },
                    { "activityTemplate", (context, data) => { return new Activity() { Type = ActivityTypes.Message, Text = $"(Activity)en: { data.name}" }; } },
                    { "stringTemplate2", (context, data) => $"en: Yo { data.name}" }
                },
                ["fr"] = new TemplateIdMap
                {
                    { "stringTemplate", (context, data) => $"fr: { data.name}" },
                    { "activityTemplate", (context, data) => { return new Activity() { Type = ActivityTypes.Message, Text = $"(Activity)fr: { data.name}" }; } },
                    { "stringTemplate2", (context, data) => $"fr: Yo { data.name}" }
                }
            };
            templates2 = new TemplateDictionary
            {
                ["en"] = new TemplateIdMap
                {
                    { "stringTemplate2", (context, data) => $"en: StringTemplate2 override {data.name}" }
                }
            };
        }



        [TestMethod]
        public async Task Template_TemplateManager_Registration()
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
        public async Task Template_TemplateManager_MultiTemplate()
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
        public async Task Template_DictionaryTemplateEngine_SimpleStringBinging()
        {
            var engine = new DictionaryRenderer(templates1);
            var result = await engine.RenderTemplate(null, "en", "stringTemplate", new { name="joe" });
            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreEqual("en: joe", (string)result);
        }

        [TestMethod]
        public async Task Template_DictionaryTemplateEngine_SimpleActivityBinding()
        {
            var engine = new DictionaryRenderer(templates1);
            var result = await engine.RenderTemplate(null, "en", "activityTemplate", new { name = "joe" });
            Assert.IsInstanceOfType(result, typeof(Activity));
            var activity = result as Activity;
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("(Activity)en: joe", activity.Text);
        }


        [TestMethod]
        public async Task Template_Middleware_defaultlookup()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .UseTemplates(templates1)
                .UseTemplates(templates2)
                .OnReceive(async (context) =>
                {
                    context.ReplyWith(context.Request.Text.Trim(), new { name = "joe" });
                    return new ReceiveResponse(handled: true);
                });

            await adapter
                .Send("stringTemplate").AssertReply("default: joe")
                .Send("activityTemplate").AssertReply("(Activity)default: joe")
                .StartTest();
        }

        [TestMethod]
        public async Task Template_Middleware_enLookup()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .UseTemplates(templates1)
                .UseTemplates(templates2)
                .OnReceive(async (context) =>
                {
                    context.Request.Locale = "en"; // force to english
                    context.ReplyWith(context.Request.Text.Trim(), new { name = "joe" });
                    return new ReceiveResponse(handled: true);
                });

            await adapter
                .Send("stringTemplate").AssertReply("en: joe")
                .Send("activityTemplate").AssertReply("(Activity)en: joe")
                .StartTest();
        }

        [TestMethod]
        public async Task Template_Middleware_frLookup()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .UseTemplates(templates1)
                .UseTemplates(templates2)
                .OnReceive(async (context) =>
                {
                    context.Request.Locale = "fr"; // force to french
                    context.ReplyWith(context.Request.Text.Trim(), new { name = "joe" });
                    return new ReceiveResponse(handled: true);
                });

            await adapter
                .Send("stringTemplate").AssertReply("fr: joe")
                .Send("activityTemplate").AssertReply("(Activity)fr: joe")
                .StartTest();
        }

        [TestMethod]
        public async Task Template_Middleware_override()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .UseTemplates(templates1)
                .UseTemplates(templates2)
                .OnReceive(async (context) =>
                {
                    context.Request.Locale = "fr"; // force to french
                    context.ReplyWith(context.Request.Text.Trim(), new { name = "joe" });
                    return new ReceiveResponse(handled: true);
                });

            await adapter
                .Send("stringTemplate2").AssertReply("fr: Yo joe")
                .Send("activityTemplate").AssertReply("(Activity)fr: joe")
                .StartTest();
        }

        [TestMethod]
        public async Task Template_Middleware_useTemplateEngine()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .UseTemplateRenderer(new DictionaryRenderer(templates1))
                .UseTemplateRenderer(new DictionaryRenderer(templates2))
                .OnReceive(async (context) =>
                {
                    context.ReplyWith(context.Request.Text.Trim(), new { name = "joe" });
                    return new ReceiveResponse(handled: true);
                });

            await adapter
                .Send("stringTemplate").AssertReply("default: joe")
                .Send("activityTemplate").AssertReply("(Activity)default: joe")
                .StartTest();
        }


    }
}
