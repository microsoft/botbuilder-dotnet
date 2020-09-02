// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.TemplateManager.Tests
{
    [Trait("TestCategory", "Template")]
    public class TemplateManagerTests : IClassFixture<TemplateFixture>
    {
        private readonly TemplateFixture templateFixture;

        public TemplateManagerTests(TemplateFixture templateFixture)
        {
            this.templateFixture = templateFixture;
        }

        [Fact]
        public void TemplateManager_Registration()
        {
            var templateManager = new TemplateManager();
            Assert.Empty(templateManager.List());

            var templateEngine1 = new DictionaryRenderer(templateFixture.Templates1);
            var templateEngine2 = new DictionaryRenderer(templateFixture.Templates2);
            templateManager.Register(templateEngine1);
            Assert.Single(templateManager.List());

            templateManager.Register(templateEngine1);
            Assert.Equal(1, templateManager.List().Count);

            templateManager.Register(templateEngine2);
            Assert.Equal(2, templateManager.List().Count);
        }

        [Fact]
        public void TemplateManager_MultiTemplate()
        {
            var templateManager = new TemplateManager();
            Assert.Empty(templateManager.List());

            var templateEngine1 = new DictionaryRenderer(templateFixture.Templates1);
            var templateEngine2 = new DictionaryRenderer(templateFixture.Templates2);
            templateManager.Register(templateEngine1);
            Assert.Single(templateManager.List());

            templateManager.Register(templateEngine1);
            Assert.Equal(1, templateManager.List().Count);

            templateManager.Register(templateEngine2);
            Assert.Equal(2, templateManager.List().Count);
        }

        [Fact]
        public async Task DictionaryTemplateEngine_SimpleStringBinding()
        {
            var engine = new DictionaryRenderer(templateFixture.Templates1);
            var result = await engine.RenderTemplate(null, "en", "stringTemplate", new { name = "joe" });
            Assert.Equal(typeof(string), result.GetType());
            Assert.Equal("en: joe", (string)result);
        }

        [Fact]
        public async Task DictionaryTemplateEngine_SimpleActivityBinding()
        {
            var engine = new DictionaryRenderer(templateFixture.Templates1);
            var result = await engine.RenderTemplate(null, "en", "activityTemplate", new { name = "joe" });
            Assert.Equal(typeof(Activity), result.GetType());

            var activity = result as Activity;
            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal("(Activity)en: joe", activity.Text);
        }

        [Fact]
        public async Task TemplateManager_defaultlookup()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(TemplateManager_defaultlookup)))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            var templateManager = new TemplateManager()
                .Register(new DictionaryRenderer(templateFixture.Templates1))
                .Register(new DictionaryRenderer(templateFixture.Templates2));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    var templateId = context.Activity.AsMessageActivity().Text.Trim();
                    await templateManager.ReplyWith(context, templateId, new { name = "joe" });
                })
                .Send("stringTemplate").AssertReply("default: joe")
                .Send("activityTemplate").AssertReply("(Activity)default: joe")
                .StartTestAsync();
        }

        [Fact]
        public async Task TemplateManager_DataDefined()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(TemplateManager_DataDefined)))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            var templateManager = new TemplateManager()
            {
                Renderers =
                {
                    new DictionaryRenderer(templateFixture.Templates1),
                    new DictionaryRenderer(templateFixture.Templates2)
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

        [Fact]
        public async Task TemplateManager_enLookup()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(TemplateManager_enLookup)))
                                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            var templateManager = new TemplateManager()
                .Register(new DictionaryRenderer(templateFixture.Templates1))
                .Register(new DictionaryRenderer(templateFixture.Templates2));

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

        [Fact]
        public async Task TemplateManager_frLookup()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(TemplateManager_frLookup)))
                                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            var templateManager = new TemplateManager()
                .Register(new DictionaryRenderer(templateFixture.Templates1))
                .Register(new DictionaryRenderer(templateFixture.Templates2));

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

        [Fact]
        public async Task TemplateManager_override()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(TemplateManager_override)))
                                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            var templateManager = new TemplateManager()
                .Register(new DictionaryRenderer(templateFixture.Templates1))
                .Register(new DictionaryRenderer(templateFixture.Templates2));

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

        [Fact]
        public async Task TemplateManager_useTemplateEngine()
        {
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(TemplateManager_useTemplateEngine)))
                                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            var templateManager = new TemplateManager()
                .Register(new DictionaryRenderer(templateFixture.Templates1))
                .Register(new DictionaryRenderer(templateFixture.Templates2));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    var templateId = context.Activity.AsMessageActivity().Text.Trim();
                    await templateManager.ReplyWith(context, templateId, new { name = "joe" });
                })
                .Send("stringTemplate").AssertReply("default: joe")
                .Send("activityTemplate").AssertReply("(Activity)default: joe")
                .StartTestAsync();
        }
    }

#pragma warning disable SA1402
    public class TemplateFixture : IDisposable
    {
        public TemplateFixture()
        {
            Templates1 = new LanguageTemplateDictionary
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
            Templates2 = new LanguageTemplateDictionary
            {
                ["en"] = new TemplateIdMap
                {
                    { "stringTemplate2", (context, data) => $"en: StringTemplate2 override {data.name}" },
                },
            };
        }

        public LanguageTemplateDictionary Templates1 { get; private set; }

        public LanguageTemplateDictionary Templates2 { get; private set; }

        public void Dispose()
        {
            Templates1 = null;
            Templates2 = null;
        }
    }
}
