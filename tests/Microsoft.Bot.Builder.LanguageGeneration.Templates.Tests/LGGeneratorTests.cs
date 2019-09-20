#pragma warning disable SA1402
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.LanguageGeneration.Generators;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class LGGeneratorTests
    {
        private static ResourceExplorer resourceExplorer;

        private readonly ImportResolverDelegate resourceResolver = LanguageGeneratorManager.ResourceResolver(resourceExplorer);

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
            var lg = new TemplateEngineLanguageGenerator(string.Empty, "test", resourceResolver);
            await lg.Generate(context, "[tesdfdfsst]", null);
        }

        [TestMethod]
        public async Task TestImport()
        {
            var languageGeneratorManager = new LanguageGeneratorManager(resourceExplorer);
            var generator = languageGeneratorManager.LanguageGenerators["import.lg"];
            var result = await generator.Generate(GetTurnContext(string.Empty), "[test2]", null);
            Assert.AreEqual("default2", result);
        }

        [TestMethod]
        public async Task TestMultiLanguageGenerator()
        {
            var lg = new MultiLanguageGenerator();
            lg.LanguageGenerators[string.Empty] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.lg").ReadTextAsync().Result, "test.lg", resourceResolver);
            lg.LanguageGenerators["de"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.de.lg").ReadTextAsync().Result, "test.de.lg", resourceResolver);
            lg.LanguageGenerators["en"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en.lg").ReadTextAsync().Result, "test.en.lg", resourceResolver);
            lg.LanguageGenerators["en-US"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en-US.lg").ReadTextAsync().Result, "test.en-US.lg", resourceResolver);
            lg.LanguageGenerators["en-GB"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en-GB.lg").ReadTextAsync().Result, "test.en-GB.lg", resourceResolver);
            lg.LanguageGenerators["fr"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.fr.lg").ReadTextAsync().Result, "test.fr.lg", resourceResolver);

            // test targeted in each language
            Assert.AreEqual("english-us", await lg.Generate(GetTurnContext(locale: "en-us"), "[test]", null));
            Assert.AreEqual("english-gb", await lg.Generate(GetTurnContext(locale: "en-gb"), "[test]", null));
            Assert.AreEqual("english", await lg.Generate(GetTurnContext(locale: "en"), "[test]", null));
            Assert.AreEqual("default", await lg.Generate(GetTurnContext(locale: string.Empty), "[test]", null));
            Assert.AreEqual("default", await lg.Generate(GetTurnContext(locale: "foo"), "[test]", null));

            // test fallback for en-us -> en -> default
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(locale: "en-us"), "[test2]", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(locale: "en-gb"), "[test2]", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(locale: "en"), "[test2]", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(locale: string.Empty), "[test2]", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(locale: "foo"), "[test2]", null));
        }

        [TestMethod]
        public async Task TestResourceMultiLanguageGenerator()
        {
            var lg = new ResourceMultiLanguageGenerator("test.lg");

            // test targeted in each language
            Assert.AreEqual("english-us", await lg.Generate(GetTurnContext("en-us", lg), "[test]", null));
            Assert.AreEqual("english-us", await lg.Generate(GetTurnContext("en-us", lg), "[test2]", new { country = "us" }));
            Assert.AreEqual("english-gb", await lg.Generate(GetTurnContext("en-gb", lg), "[test]", null));
            Assert.AreEqual("english", await lg.Generate(GetTurnContext("en", lg), "[test]", null));
            Assert.AreEqual("default", await lg.Generate(GetTurnContext(string.Empty, lg), "[test]", null));
            Assert.AreEqual("default", await lg.Generate(GetTurnContext("foo", lg), "[test]", null));

            // test fallback for en-us -> en -> default
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext("en-us", lg), "[test2]", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext("en-gb", lg), "[test2]", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext("en", lg), "[test2]", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext(string.Empty, lg), "[test2]", null));
            Assert.AreEqual("default2", await lg.Generate(GetTurnContext("foo", lg), "[test2]", null));
        }

        [TestMethod]
        public async Task TestLanguageGeneratorMiddleware()
        {
            await CreateFlow("en-us", async (turnContext, cancellationToken) =>
            {
                var lg = turnContext.TurnState.Get<ILanguageGenerator>();
                Assert.IsNotNull(lg, "ILanguageGenerator should not be null");
                Assert.IsNotNull(turnContext.TurnState.Get<ResourceExplorer>(), "ResourceExplorer should not be null");
                var text = await lg.Generate(turnContext, "[test]", null);
                Assert.AreEqual("english-us", text, "template should be there");
            })
            .Send("hello")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestDialogInjection()
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
                            new SendActivity("[test]")
                        }
                    }
                }
            };
            DialogManager dm = new DialogManager(dialog);

            await CreateFlow("en-us", async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            })
            .Send("hello")
                .AssertReply("overriden")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestDialogInjectionDeclarative()
        {
            await CreateFlow("en-us", async (turnContext, cancellationToken) =>
            {
                var resource = resourceExplorer.GetResource("test.dialog");
                var dialog = (AdaptiveDialog)DeclarativeTypeLoader.Load<Dialog>(resource, resourceExplorer, DebugSupport.SourceRegistry);
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
                var result = await lg.Generate(turnContext, "This is {test.name}", new
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
