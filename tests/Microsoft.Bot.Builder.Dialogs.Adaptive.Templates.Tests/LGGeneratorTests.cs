#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1515 // Single-line comment should be preceded by blank line
#pragma warning disable SA1402
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class LGGeneratorTests
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

        [ClassCleanup]
        public static void ClassCleanup()
        {
            resourceExplorer.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TestNotFoundTemplate()
        {
            var context = GetTurnContext(string.Empty);
            var lg = new TemplateEngineLanguageGenerator();
            await lg.Generate(context, "@{tesdfdfsst}", null);
        }

        [TestMethod]
        public async Task TestMultiLangImport()
        {
            var languageGeneratorManager = new LanguageGeneratorManager(resourceExplorer);
            var generator = languageGeneratorManager.LanguageGenerators["common.lg"];
            var templateContent = "@{templateb()}";

            var result = await generator.Generate(GetTurnContext(string.Empty), templateContent, null);
            Assert.AreEqual("from b.lg", result);

            // fallback to default
            result = await generator.Generate(GetTurnContext(locale: "foo"), templateContent, null);
            Assert.AreEqual("from b.lg", result);

            result = await generator.Generate(GetTurnContext(locale: "en-us"), templateContent, null);
            Assert.AreEqual("from b.en-us.lg", result);

            // fallback to en
            result = await generator.Generate(GetTurnContext(locale: "en-gb"), templateContent, null);
            Assert.AreEqual("from b.en.lg", result);

            result = await generator.Generate(GetTurnContext(locale: "en"), templateContent, null);
            Assert.AreEqual("from b.en.lg", result);
        }

        [TestMethod]
        public async Task TestMultiLanguageE2E()
        {
            await CreateMultiLanguageFlow(async (turnContext, cancellationToken) =>
            {
                var lg = turnContext.TurnState.Get<ILanguageGenerator>();
                Assert.IsNotNull(lg, "ILanguageGenerator should not be null");
                Assert.IsNotNull(turnContext.TurnState.Get<ResourceExplorer>(), "ResourceExplorer should not be null");

                turnContext.Activity.Locale = string.Empty;
                var text = await lg.Generate(turnContext, "@{templatea()}", null);
                Assert.AreEqual("from a.lg", text, "template should be there");
                text = await lg.Generate(turnContext, "@{templateb()}", null);
                Assert.AreEqual("from b.lg", text, "template should be there");

                turnContext.Activity.Locale = "en-us";
                text = await lg.Generate(turnContext, "@{templatea()}", null);
                Assert.AreEqual("from a.en-US.lg", text, "template should be there");
                text = await lg.Generate(turnContext, "@{templateb()}", null);
                Assert.AreEqual("from b.en-us.lg", text, "template should be there");

                turnContext.Activity.Locale = "en";
                text = await lg.Generate(turnContext, "@{templatea()}", null);
                Assert.AreEqual("from a.lg", text, "template should be there");
                text = await lg.Generate(turnContext, "@{templateb()}", null);
                Assert.AreEqual("from b.en.lg", text, "template should be there");

                turnContext.Activity.Locale = "foo";
                text = await lg.Generate(turnContext, "@{templatea()}", null);
                Assert.AreEqual("from a.lg", text, "template should be there");
                text = await lg.Generate(turnContext, "@{templateb()}", null);
                Assert.AreEqual("from b.lg", text, "template should be there");
            })
            .Send("hello")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestMultiLanguageGenerator()
        {
            var lg = new MultiLanguageGenerator();
            var multilanguageresources = MultiLanguageResourceLoader.Load(resourceExplorer);
            lg.LanguageGenerators[string.Empty] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.lg").ReadTextAsync().Result, "test.lg", multilanguageresources);
            lg.LanguageGenerators["de"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.de.lg").ReadTextAsync().Result, "test.de.lg", multilanguageresources);
            lg.LanguageGenerators["en"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en.lg").ReadTextAsync().Result, "test.en.lg", multilanguageresources);
            lg.LanguageGenerators["en-US"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en-US.lg").ReadTextAsync().Result, "test.en-US.lg", multilanguageresources);
            lg.LanguageGenerators["en-GB"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en-GB.lg").ReadTextAsync().Result, "test.en-GB.lg", multilanguageresources);
            lg.LanguageGenerators["fr"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.fr.lg").ReadTextAsync().Result, "test.fr.lg", multilanguageresources);

            // test targeted in each language
            Assert.AreEqual("english-us", await lg.Generate(GetTurnContext(locale: "en-us"), "@{test()}", null));
            Assert.AreEqual("english-gb", await lg.Generate(GetTurnContext(locale: "en-gb"), "@{test()}", null));
            Assert.AreEqual("english", await lg.Generate(GetTurnContext(locale: "en"), "@{test()}", null));
            Assert.AreEqual("default", await lg.Generate(GetTurnContext(locale: string.Empty), "@{test()}", null));
            Assert.AreEqual("default", await lg.Generate(GetTurnContext(locale: "foo"), "@{test()}", null));

            // test fallback for en-us -> en -> default
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(locale: "en-us"), "@{test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(locale: "en-gb"), "@{test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(locale: "en"), "@{test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(locale: string.Empty), "@{test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(locale: "foo"), "@{test2()}", null));
        }

        [TestMethod]
        public async Task TestResourceMultiLanguageGenerator()
        {
            var lg = new ResourceMultiLanguageGenerator("test.lg");

            // test targeted in each language
            Assert.AreEqual("english-us", await lg.Generate(GetTurnContext("en-us", lg), "@{test()}", null));
            Assert.AreEqual("english-us", await lg.Generate(GetTurnContext("en-us", lg), "@{test()}", new { country = "us" }));
            Assert.AreEqual("english-gb", await lg.Generate(GetTurnContext("en-gb", lg), "@{test()}", null));
            Assert.AreEqual("english", await lg.Generate(GetTurnContext("en", lg), "@{test()}", null));
            Assert.AreEqual("default", await lg.Generate(GetTurnContext(string.Empty, lg), "@{test()}", null));
            Assert.AreEqual("default", await lg.Generate(GetTurnContext("foo", lg), "@{test()}", null));

            // test fallback for en-us -> en -> default
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext("en-us", lg), "@{test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext("en-gb", lg), "@{test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext("en", lg), "@{test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(string.Empty, lg), "@{test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext("foo", lg), "@{test2()}", null));
        }

        [TestMethod]
        public async Task TestLanguageGeneratorMiddleware()
        {
            await CreateFlow("en-us", async (turnContext, cancellationToken) =>
            {
                var lg = turnContext.TurnState.Get<ILanguageGenerator>();
                Assert.IsNotNull(lg, "ILanguageGenerator should not be null");
                Assert.IsNotNull(turnContext.TurnState.Get<ResourceExplorer>(), "ResourceExplorer should not be null");
                var text = await lg.Generate(turnContext, "@{test()}", null);
                Assert.AreEqual("english-us", text, "template should be there");
            })
            .Send("hello")
            .StartTestAsync();
        }

        internal class AssertLGDialog : Dialog
        {
            public AssertLGDialog()
            {
            }

            public string ResourceId { get; set; }

            public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                var generator = (ResourceMultiLanguageGenerator)dc.Context.TurnState.Get<ILanguageGenerator>();
                Assert.AreEqual(ResourceId, generator.ResourceId);
                await dc.Context.SendActivityAsync(generator.ResourceId);
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }

            public async override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
            {
                var generator = (ResourceMultiLanguageGenerator)dc.Context.TurnState.Get<ILanguageGenerator>();
                Assert.AreEqual(ResourceId, generator.ResourceId);
                await dc.Context.SendActivityAsync(generator.ResourceId);
                return await dc.EndDialogAsync();
            }
        }

        [TestMethod]
        public async Task TestLGScopedAccess()
        {
            var dialog = new AdaptiveDialog()
            {
                Generator = new ResourceMultiLanguageGenerator("subDialog.lg"),
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new AssertLGDialog() { ResourceId = "subDialog.lg" },
                            new BeginDialog()
                            {
                                Dialog = new AdaptiveDialog()
                                {
                                    Generator = new ResourceMultiLanguageGenerator("test.lg"),
                                    Triggers = new List<OnCondition>()
                                    {
                                        new OnBeginDialog()
                                        {
                                            Actions = new List<Dialog>()
                                            {
                                                new AssertLGDialog() { ResourceId = "test.lg" },
                                            }
                                        }
                                    }
                                }
                            },
                            new AssertLGDialog() { ResourceId = "subDialog.lg" },
                        }
                    }
                }
            };

            DialogManager dm = new DialogManager(dialog);
            await CreateFlow("en-us", async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            })
            .Send("test")
                // BeginDialog() outer dialog should be subDialog.lg
                .AssertReply("subDialog.lg")
            .Send("test")
                // ContinueDialog() outer dialog should be subDialog.lg
                .AssertReply("subDialog.lg")
                // BeginDialog() on inner dialog should be test.lg
                .AssertReply("test.lg")
            .Send("test")
                // ContinueDialog() on inner dialog should be test.lg
                .AssertReply("test.lg")
            // ResumeDialog() on outer dialog should be subDialog.lg
            .AssertReply("subDialog.lg")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestDialogInjectionDeclarative()
        {
            await CreateFlow("en-us", async (turnContext, cancellationToken) =>
            {
                var resource = resourceExplorer.GetResource("test.dialog");
                var dialog = (AdaptiveDialog)DeclarativeTypeLoader.Load<Dialog>(resource, resourceExplorer, DebugSupport.SourceMap);
                DialogManager dm = new DialogManager(dialog);
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            })
            .Send("hello")
                .AssertReply("root")
                .AssertReply("overriden")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestNoResourceExplorerLanguageGeneration()
        {
            await CreateNoResourceExplorerFlow("en-us", async (turnContext, cancellationToken) =>
            {
                var lg = turnContext.TurnState.Get<ILanguageGenerator>();
                var result = await lg.Generate(turnContext, "This is @{test.name}", new
                {
                    test = new
                    {
                        name = "Tom"
                    }
                });
                await turnContext.SendActivityAsync(result);
            })
            .Send("hello")
                .AssertReply("This is Tom")
            .StartTestAsync();
        }

        private static string GetProjectFolder()
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
        }

        private ITurnContext GetTurnContext(string locale, ILanguageGenerator generator = null)
        {
            var context = new TurnContext(
                new TestAdapter()
                .UseResourceExplorer(resourceExplorer)
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(resourceExplorer, generator ?? new MockLanguageGenerator()), new Activity() { Locale = locale, Text = string.Empty });
            context.TurnState.Add(new LanguageGeneratorManager(resourceExplorer));
            if (generator != null)
            {
                context.TurnState.Add<ILanguageGenerator>(generator);
            }

            return context;
        }

        private TestFlow CreateFlow(string locale, BotCallbackHandler handler)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .UseResourceExplorer(resourceExplorer)
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(resourceExplorer, "test.lg")
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            return new TestFlow(adapter, handler);
        }

        private TestFlow CreateMultiLanguageFlow(BotCallbackHandler handler)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .UseResourceExplorer(resourceExplorer)
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(resourceExplorer, "a.lg")
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            return new TestFlow(adapter, handler);
        }

        private TestFlow CreateNoResourceExplorerFlow(string locale, BotCallbackHandler handler)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .UseAdaptiveDialogs()
                .UseLanguageGeneration()
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            return new TestFlow(adapter, handler);
        }
    }

    public class MockLanguageGenerator : ILanguageGenerator
    {
        public Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            return Task.FromResult(template);
        }
    }
}
