#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1515 // Single-line comment should be preceded by blank line
#pragma warning disable SA1402
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class LGGeneratorTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TestNotFoundTemplate()
        {
            var context = GetDialogContext(string.Empty);
            var lg = new TemplateEngineLanguageGenerator();
            await lg.Generate(context, "${tesdfdfsst()}", null);
        }

        [TestMethod]
        public async Task TestMultiLangImport()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);

            // use LG file as entrance
            var lgResourceGroup = LGResourceLoader.GroupByLocale(resourceExplorer);

            var resource = resourceExplorer.GetResource("a.en-US.lg") as FileResource;
            var generator = new TemplateEngineLanguageGenerator(resource.FullName, lgResourceGroup);
            var result = await generator.Generate(GetDialogContext(), "${templatea()}", null);
            Assert.AreEqual("from a.en-us.lg", result);

            // import b.en-us.lg
            result = await generator.Generate(GetDialogContext(), "${templateb()}", null);
            Assert.AreEqual("from b.en-us.lg", result);

            // fallback to c.en.lg
            result = await generator.Generate(GetDialogContext(), "${templatec()}", null);
            Assert.AreEqual("from c.en.lg", result);

            // there is no 'greeting' template in b.en-us.lg, no more fallback to b.lg
            var ex = await Assert.ThrowsExceptionAsync<Exception>(async () => await generator.Generate(GetDialogContext(), "${greeting()}", null));
            Assert.IsTrue(ex.Message.Contains("greeting does not have an evaluator"));

            resource = resourceExplorer.GetResource("a.lg") as FileResource;
            generator = new TemplateEngineLanguageGenerator(resource.FullName, lgResourceGroup);

            result = await generator.Generate(GetDialogContext(), "${templatea()}", null);
            Assert.AreEqual("from a.lg", result);

            result = await generator.Generate(GetDialogContext(), "${templateb()}", null);
            Assert.AreEqual("from b.lg", result);

            // ignore the "en" in c.en.lg, just load c.lg
            result = await generator.Generate(GetDialogContext(), "${templatec()}", null);
            Assert.AreEqual("from c.lg", result);

            result = await generator.Generate(GetDialogContext(), "${greeting()}", null);
            Assert.AreEqual("hi", result);
        }

        public class TestMultiLanguageDialog : Dialog
        {
            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default)
            {
                var lg = dialogContext.Services.Get<LanguageGenerator>();

                // en-us locale
                dialogContext.Context.Activity.Locale = "en-us";
                Assert.IsNotNull(lg, "ILanguageGenerator should not be null");
                Assert.IsNotNull(dialogContext.Services.Get<ResourceExplorer>(), "ResourceExplorer should not be null");

                var result = await lg.Generate(dialogContext, "${templatea()}", null);
                Assert.AreEqual("from a.en-us.lg", result);

                // import b.en-us.lg
                result = await lg.Generate(dialogContext, "${templateb()}", null);
                Assert.AreEqual("from b.en-us.lg", result);

                // fallback to c.en.lg
                result = await lg.Generate(dialogContext, "${templatec()}", null);
                Assert.AreEqual("from c.en.lg", result);

                // there is no 'greeting' template in b.en-us.lg, fallback to a.lg to find it.
                result = await lg.Generate(dialogContext, "${greeting()}", null);
                Assert.AreEqual("hi", result);

                //en locale
                dialogContext.Context.Activity.Locale = "en";
                Assert.IsNotNull(lg, "ILanguageGenerator should not be null");
                Assert.IsNotNull(dialogContext.Services.Get<ResourceExplorer>(), "ResourceExplorer should not be null");

                result = await lg.Generate(dialogContext, "${templatea()}", null);
                Assert.AreEqual("from a.lg", result);

                // import b.en-us.lg
                result = await lg.Generate(dialogContext, "${templateb()}", null);
                Assert.AreEqual("from b.lg", result);

                // c.en.lg is ignore in b.lg
                result = await lg.Generate(dialogContext, "${templatec()}", null);
                Assert.AreEqual("from c.lg", result);

                // there is no 'greeting' template in b.en-us.lg, fallback to a.lg to find it.
                result = await lg.Generate(dialogContext, "${greeting()}", null);
                Assert.AreEqual("hi", result);

                // empty locale
                dialogContext.Context.Activity.Locale = string.Empty;
                result = await lg.Generate(dialogContext, "${templatea()}", null);
                Assert.AreEqual("from a.lg", result);

                result = await lg.Generate(dialogContext, "${templateb()}", null);
                Assert.AreEqual("from b.lg", result);

                // ignore the "en" in c.en.lg, just load c.lg
                result = await lg.Generate(dialogContext, "${templatec()}", null);
                Assert.AreEqual("from c.lg", result);

                result = await lg.Generate(dialogContext, "${greeting()}", null);
                Assert.AreEqual("hi", result);

                return await dialogContext.EndDialogAsync();
            }
        }

        [TestMethod]
        public async Task TestMultiLanguageE2E()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);
            DialogManager dm = new DialogManager()
            {
                RootDialog = new TestMultiLanguageDialog()
            }
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration("a.lg");

            await CreateFlow(async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken);
            })
            .Send("hello")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestMultiLanguageGenerator()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);

            var lg = new MultiLanguageGenerator();
            var multilanguageresources = LGResourceLoader.GroupByLocale(resourceExplorer);
            lg.LanguageGenerators[string.Empty] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.lg").ReadTextAsync().Result, "test.lg", multilanguageresources);
            lg.LanguageGenerators["de"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.de.lg").ReadTextAsync().Result, "test.de.lg", multilanguageresources);
            lg.LanguageGenerators["en"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en.lg").ReadTextAsync().Result, "test.en.lg", multilanguageresources);
            lg.LanguageGenerators["en-US"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en-US.lg").ReadTextAsync().Result, "test.en-US.lg", multilanguageresources);
            lg.LanguageGenerators["en-GB"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en-GB.lg").ReadTextAsync().Result, "test.en-GB.lg", multilanguageresources);
            lg.LanguageGenerators["fr"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.fr.lg").ReadTextAsync().Result, "test.fr.lg", multilanguageresources);

            // test targeted in each language
            Assert.AreEqual("english-us", await lg.Generate(GetDialogContext(locale: "en-us"), "${test()}", null));
            Assert.AreEqual("english-gb", await lg.Generate(GetDialogContext(locale: "en-gb"), "${test()}", null));
            Assert.AreEqual("english", await lg.Generate(GetDialogContext(locale: "en"), "${test()}", null));
            Assert.AreEqual("default", await lg.Generate(GetDialogContext(locale: string.Empty), "${test()}", null));
            Assert.AreEqual("default", await lg.Generate(GetDialogContext(locale: "foo"), "${test()}", null));

            // test fallback for en-us -> en -> default
            //Assert.AreEqual("default2", await lg.Generate(GetTurnContext(locale: "en-us"), "${test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetDialogContext(locale: "en-gb"), "${test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetDialogContext(locale: "en"), "${test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetDialogContext(locale: string.Empty), "${test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetDialogContext(locale: "foo"), "${test2()}", null));
        }

        [TestMethod]
        public async Task TestResourceMultiLanguageGenerator()
        {
            var lg = new ResourceMultiLanguageGenerator("test.lg");

            // test targeted in each language
            Assert.AreEqual("english-us", await lg.Generate(GetDialogContext("en-us", lg), "${test()}", null));
            Assert.AreEqual("english-us", await lg.Generate(GetDialogContext("en-us", lg), "${test()}", new { country = "us" }));
            Assert.AreEqual("english-gb", await lg.Generate(GetDialogContext("en-gb", lg), "${test()}", null));
            Assert.AreEqual("english", await lg.Generate(GetDialogContext("en", lg), "${test()}", null));
            Assert.AreEqual("default", await lg.Generate(GetDialogContext(string.Empty, lg), "${test()}", null));
            Assert.AreEqual("default", await lg.Generate(GetDialogContext("foo", lg), "${test()}", null));

            // test fallback for en-us -> en -> default
            //Assert.AreEqual("default2", await lg.Generate(GetTurnContext("en-us", lg), "${test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetDialogContext("en-gb", lg), "${test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetDialogContext("en", lg), "${test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetDialogContext(string.Empty, lg), "${test2()}", null));
            Assert.AreEqual("default2", await lg.Generate(GetDialogContext("foo", lg), "${test2()}", null));
        }

        public class TestLanguageGeneratorMiddlewareDialog : Dialog
        {
            public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default)
            {
                var lg = dialogContext.Services.Get<LanguageGenerator>();
                Assert.IsNotNull(lg, "ILanguageGenerator should not be null");
                Assert.IsNotNull(dialogContext.Services.Get<ResourceExplorer>(), "ResourceExplorer should not be null");
                var text = await lg.Generate(dialogContext, "${test()}", null);
                Assert.AreEqual("english-us", text, "template should be there");
                return await dialogContext.EndDialogAsync();
            }
        }

        [TestMethod]
        public async Task TestLanguageGeneratorMiddleware()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);
            var dm = new DialogManager()
            {
                RootDialog = new TestLanguageGeneratorMiddlewareDialog()
            }
               .UseResourceExplorer(resourceExplorer)
               .UseLanguageGeneration("test.lg");

            await CreateFlow(async (context, cancellationToken) =>
            {
                await dm.OnTurnAsync(context, cancellationToken);
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
                var generator = (ResourceMultiLanguageGenerator)dc.Services.Get<LanguageGenerator>();
                Assert.AreEqual(ResourceId, generator.ResourceId);
                await dc.Context.SendActivityAsync($"BeginDialog {Id}:{generator.ResourceId}");
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }

            public async override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
            {
                var generator = (ResourceMultiLanguageGenerator)dc.Services.Get<LanguageGenerator>();
                Assert.AreEqual(ResourceId, generator.ResourceId);
                await dc.Context.SendActivityAsync($"ContinueDialog {Id}:{generator.ResourceId}");
                return await dc.EndDialogAsync();
            }
        }

        [TestMethod]
        public async Task TestLGScopedAccess()
        {
            var dialog = new AdaptiveDialog()
            {
                Id = "AdaptiveDialog1",
                Generator = new ResourceMultiLanguageGenerator("subDialog.lg"),
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new AssertLGDialog() { Id = "test1", ResourceId = "subDialog.lg" },
                            new BeginDialog()
                            {
                                Dialog = new AdaptiveDialog()
                                {
                                    Id = "AdaptiveDialog2",
                                    Generator = new ResourceMultiLanguageGenerator("test.lg"),
                                    Triggers = new List<OnCondition>()
                                    {
                                        new OnBeginDialog()
                                        {
                                            Actions = new List<Dialog>()
                                            {
                                                new AssertLGDialog() { Id = "test2", ResourceId = "test.lg" },
                                            }
                                        }
                                    }
                                }
                            },
                            new AssertLGDialog() { Id = "test3", ResourceId = "subDialog.lg" },
                            new SendActivity("Done")
                        }
                    }
                }
            };
            var resourceExplorer = new ResourceExplorer()
                .LoadProject(GetProjectFolder(), monitorChanges: false);

            DialogManager dm = new DialogManager(dialog)
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration("test.lg");
            await CreateFlow(async (turnContext, cancellationToken) =>
            {
                System.Diagnostics.Trace.TraceInformation($"BEGIN TURN {turnContext.Activity.Text}");
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
                System.Diagnostics.Trace.TraceInformation($"END TURN {turnContext.Activity.Text}");
            })
            // inside AdaptiveDialog1
            .Send("turn1")
                .AssertReply("BeginDialog test1:subDialog.lg")
            .Send("turn2")
                .AssertReply("ContinueDialog test1:subDialog.lg")
                // inside AdaptiveDialog2
                .AssertReply("BeginDialog test2:test.lg")
            .Send("turn3")
                .AssertReply("ContinueDialog test2:test.lg")
                // back out to AdaptiveDialog1
                .AssertReply("BeginDialog test3:subDialog.lg")
            .Send("turn4")
                .AssertReply("ContinueDialog test3:subDialog.lg")
                .AssertReply("Done")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestDialogInjectionDeclarative()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);
            DialogManager dm = new DialogManager()
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration("test.lg")
                .UseLanguagePolicy(new LanguagePolicy("fr-fr"));
            dm.RootDialog = (AdaptiveDialog)resourceExplorer.LoadType<Dialog>("test.dialog");

            await CreateFlow(async (dialogContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(dialogContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            })
            .Send("hello")
                .AssertReply("root")
                .AssertReply("overriden in fr")
            .StartTestAsync();
        }

        public class TestNoResourceExplorerDialog : Dialog
        {
            public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default)
            {
                var lg = dialogContext.Services.Get<LanguageGenerator>();

                var result = await lg.Generate(dialogContext, "This is ${test.name}", new
                {
                    test = new
                    {
                        name = "Tom"
                    }
                });

                await dialogContext.Context.SendActivityAsync(result);

                return await dialogContext.EndDialogAsync();
            }
        }

        [TestMethod]
        public async Task TestNoResourceExplorerLanguageGeneration()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);
            DialogManager dm = new DialogManager()
            {
                RootDialog = new TestNoResourceExplorerDialog()
            }
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration();

            await CreateNoResourceExplorerFlow(async (context, cancellationToken) =>
            {
                await dm.OnTurnAsync(context, cancellationToken);
            })
            .Send("hello")
                .AssertReply("This is Tom")
            .StartTestAsync();
        }

        private static string GetProjectFolder()
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
        }

        private DialogContext GetDialogContext(string locale = null, LanguageGenerator generator = null)
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);

            var context = new TurnContext(new TestAdapter(), new Activity() { Locale = locale ?? string.Empty, Text = string.Empty });
            context.TurnState.Add(resourceExplorer);
            context.TurnState.Add(new LanguageGeneratorManager(resourceExplorer));
            generator = generator ?? new MockLanguageGenerator();
            if (generator != null)
            {
                context.TurnState.Add<LanguageGenerator>(generator);
            }

            var dc = new DialogContext(new DialogSet(), context, new DialogState());

            foreach (var service in context.TurnState)
            {
                dc.Services[service.Key] = service.Value;
            }

            return dc;
        }

        private TestFlow CreateFlow(BotCallbackHandler handler)
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            return new TestFlow(adapter, handler);
        }

        private TestFlow CreateNoResourceExplorerFlow(BotCallbackHandler handler)
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            return new TestFlow(adapter, handler);
        }
    }

    public class MockLanguageGenerator : LanguageGenerator
    {
        public override Task<string> Generate(DialogContext dialogContext, string template, object data)
        {
            return Task.FromResult(template);
        }
    }
}
