#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1515 // Single-line comment should be preceded by blank line
#pragma warning disable SA1402
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    public class LGGeneratorTests
    {
        public LGGeneratorTests()
        {
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new AdaptiveTestingComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
        }

        [Fact]
        public async Task TestNotFoundTemplate()
        {
            var context = GetDialogContext(string.Empty);
            var lg = new TemplateEngineLanguageGenerator();
            await Assert.ThrowsAsync<Exception>(() => lg.GenerateAsync(context, "${tesdfdfsst()}", null));
        }

        [Fact]
        public void TestLGResourceGroup()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);

            // use LG file as entrance
            var lgResourceGroup = LGResourceLoader.GroupByLocale(resourceExplorer);

            Assert.Contains(string.Empty, lgResourceGroup.Keys.ToList());
            var resourceNames = lgResourceGroup[string.Empty].Select(u => u.Id);
            Assert.Equal(8, resourceNames.Count());
            Assert.Subset(new HashSet<string>() { "a.lg", "b.lg", "c.lg", "inject.lg", "NormalStructuredLG.lg", "root.lg", "subDialog.lg", "test.lg" }, new HashSet<string>(resourceNames));

            Assert.Contains("en-us", lgResourceGroup.Keys.ToList());
            resourceNames = lgResourceGroup["en-us"].Select(u => u.Id);
            Assert.Equal(8, resourceNames.Count());
            Assert.Subset(new HashSet<string>() { "a.en-US.lg", "b.en-us.lg", "c.en.lg", "inject.lg", "NormalStructuredLG.lg", "root.lg", "subDialog.lg", "test.en-US.lg" }, new HashSet<string>(resourceNames));

            Assert.Contains("en", lgResourceGroup.Keys.ToList());
            resourceNames = lgResourceGroup["en"].Select(u => u.Id);
            Assert.Equal(8, resourceNames.Count());
            Assert.Subset(new HashSet<string>() { "a.lg", "b.lg", "c.en.lg", "inject.lg", "NormalStructuredLG.lg", "root.lg", "subDialog.lg", "test.en.lg" }, new HashSet<string>(resourceNames));
        }

        [Fact]
        public async Task TestMultiLangImport()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);

            // use LG file as entrance
            var lgResourceGroup = LGResourceLoader.GroupByLocale(resourceExplorer);

            var resource = resourceExplorer.GetResource("a.en-US.lg") as FileResource;
            var generator = new TemplateEngineLanguageGenerator(resource, lgResourceGroup);
            var result = await generator.GenerateAsync(GetDialogContext(), "${templatea()}", null);
            Assert.Equal("from a.en-us.lg", result);

            // import b.en-us.lg
            result = await generator.GenerateAsync(GetDialogContext(), "${templateb()}", null);
            Assert.Equal("from b.en-us.lg", result);

            // fallback to c.en.lg
            result = await generator.GenerateAsync(GetDialogContext(), "${templatec()}", null);
            Assert.Equal("from c.en.lg", result);

            // there is no 'greeting' template in b.en-us.lg, no more fallback to b.lg
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await generator.GenerateAsync(GetDialogContext(), "${greeting()}", null));
            Assert.Contains("greeting does not have an evaluator", ex.Message);

            resource = resourceExplorer.GetResource("a.lg") as FileResource;
            generator = new TemplateEngineLanguageGenerator(resource, lgResourceGroup);

            result = await generator.GenerateAsync(GetDialogContext(), "${templatea()}", null);
            Assert.Equal("from a.lg", result);

            result = await generator.GenerateAsync(GetDialogContext(), "${templateb()}", null);
            Assert.Equal("from b.lg", result);

            // ignore the "en" in c.en.lg, just load c.lg
            result = await generator.GenerateAsync(GetDialogContext(), "${templatec()}", null);
            Assert.Equal("from c.lg", result);

            result = await generator.GenerateAsync(GetDialogContext(), "${greeting()}", null);
            Assert.Equal("hi", result);
        }

        public class TestMultiLanguageDialog : Dialog
        {
            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default)
            {
                var lg = dialogContext.Services.Get<LanguageGenerator>();

                // en-us locale
                dialogContext.Context.Activity.Locale = "en-us";
                Assert.NotNull(lg);
                Assert.NotNull(dialogContext.Services.Get<ResourceExplorer>());

                var result = await lg.GenerateAsync(dialogContext, "${templatea()}", null);
                Assert.Equal("from a.en-us.lg", result);

                // import b.en-us.lg
                result = await lg.GenerateAsync(dialogContext, "${templateb()}", null);
                Assert.Equal("from b.en-us.lg", result);

                // fallback to c.en.lg
                result = await lg.GenerateAsync(dialogContext, "${templatec()}", null);
                Assert.Equal("from c.en.lg", result);

                // there is no 'greeting' template in b.en-us.lg, fallback to a.lg to find it.
                result = await lg.GenerateAsync(dialogContext, "${greeting()}", null);
                Assert.Equal("hi", result);

                //en locale
                dialogContext.Context.Activity.Locale = "en";
                Assert.NotNull(lg);
                Assert.NotNull(dialogContext.Services.Get<ResourceExplorer>());

                result = await lg.GenerateAsync(dialogContext, "${templatea()}", null);
                Assert.Equal("from a.lg", result);

                // import b.en-us.lg
                result = await lg.GenerateAsync(dialogContext, "${templateb()}", null);
                Assert.Equal("from b.lg", result);

                // c.en.lg is ignore in b.lg
                result = await lg.GenerateAsync(dialogContext, "${templatec()}", null);
                Assert.Equal("from c.lg", result);

                // there is no 'greeting' template in b.en-us.lg, fallback to a.lg to find it.
                result = await lg.GenerateAsync(dialogContext, "${greeting()}", null);
                Assert.Equal("hi", result);

                // empty locale
                dialogContext.Context.Activity.Locale = string.Empty;
                result = await lg.GenerateAsync(dialogContext, "${templatea()}", null);
                Assert.Equal("from a.lg", result);

                result = await lg.GenerateAsync(dialogContext, "${templateb()}", null);
                Assert.Equal("from b.lg", result);

                // ignore the "en" in c.en.lg, just load c.lg
                result = await lg.GenerateAsync(dialogContext, "${templatec()}", null);
                Assert.Equal("from c.lg", result);

                result = await lg.GenerateAsync(dialogContext, "${greeting()}", null);
                Assert.Equal("hi", result);

                return await dialogContext.EndDialogAsync();
            }
        }

        [Fact]
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

        [Fact]
        public async Task TestMultiLanguageGenerator()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);

            var lg = new MultiLanguageGenerator();
            var multilanguageresources = LGResourceLoader.GroupByLocale(resourceExplorer);
            lg.LanguageGenerators[string.Empty] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.lg"), multilanguageresources);
            lg.LanguageGenerators["de"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.de.lg"), multilanguageresources);
            lg.LanguageGenerators["en"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en.lg"), multilanguageresources);
            lg.LanguageGenerators["en-US"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en-US.lg"), multilanguageresources);
            lg.LanguageGenerators["en-GB"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.en-GB.lg"), multilanguageresources);
            lg.LanguageGenerators["fr"] = new TemplateEngineLanguageGenerator(resourceExplorer.GetResource("test.fr.lg"),  multilanguageresources);

            // test targeted in each language
            Assert.Equal("english-us", await lg.GenerateAsync(GetDialogContext(locale: "en-us"), "${test()}", null));
            Assert.Equal("english-gb", await lg.GenerateAsync(GetDialogContext(locale: "en-gb"), "${test()}", null));
            Assert.Equal("english", await lg.GenerateAsync(GetDialogContext(locale: "en"), "${test()}", null));
            Assert.Equal("default", await lg.GenerateAsync(GetDialogContext(locale: string.Empty), "${test()}", null));
            Assert.Equal("default", await lg.GenerateAsync(GetDialogContext(locale: "foo"), "${test()}", null));

            // test fallback for en-us -> en -> default
            //Assert.Equal("default2", await lg.Generate(GetTurnContext(locale: "en-us"), "${test2()}", null));
            Assert.Equal("default2", await lg.GenerateAsync(GetDialogContext(locale: "en-gb"), "${test2()}", null));
            Assert.Equal("default2", await lg.GenerateAsync(GetDialogContext(locale: "en"), "${test2()}", null));
            Assert.Equal("default2", await lg.GenerateAsync(GetDialogContext(locale: string.Empty), "${test2()}", null));
            Assert.Equal("default2", await lg.GenerateAsync(GetDialogContext(locale: "foo"), "${test2()}", null));
        }

        [Fact]
        public async Task TestResourceMultiLanguageGenerator()
        {
            var lg = new ResourceMultiLanguageGenerator("test.lg");

            // test targeted in each language
            Assert.Equal("english-us", await lg.GenerateAsync(GetDialogContext("en-us", lg), "${test()}", null));
            Assert.Equal("english-us", await lg.GenerateAsync(GetDialogContext("en-us", lg), "${test()}", new { country = "us" }));
            Assert.Equal("english-gb", await lg.GenerateAsync(GetDialogContext("en-gb", lg), "${test()}", null));
            Assert.Equal("english", await lg.GenerateAsync(GetDialogContext("en", lg), "${test()}", null));
            Assert.Equal("default", await lg.GenerateAsync(GetDialogContext(string.Empty, lg), "${test()}", null));
            Assert.Equal("default", await lg.GenerateAsync(GetDialogContext("foo", lg), "${test()}", null));

            // test fallback for en-us -> en -> default
            //Assert.Equal("default2", await lg.Generate(GetTurnContext("en-us", lg), "${test2()}", null));
            Assert.Equal("default2", await lg.GenerateAsync(GetDialogContext("en-gb", lg), "${test2()}", null));
            Assert.Equal("default2", await lg.GenerateAsync(GetDialogContext("en", lg), "${test2()}", null));
            Assert.Equal("default2", await lg.GenerateAsync(GetDialogContext(string.Empty, lg), "${test2()}", null));
            Assert.Equal("default2", await lg.GenerateAsync(GetDialogContext("foo", lg), "${test2()}", null));
        }

        public class TestLanguageGeneratorMiddlewareDialog : Dialog
        {
            public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default)
            {
                var lg = dialogContext.Services.Get<LanguageGenerator>();
                Assert.NotNull(lg);
                Assert.NotNull(dialogContext.Services.Get<ResourceExplorer>());
                var text = await lg.GenerateAsync(dialogContext, "${test()}", null);
                Assert.Equal("english-us", text);
                return await dialogContext.EndDialogAsync();
            }
        }

        [Fact]
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
                Assert.Equal(ResourceId, generator.ResourceId);
                await dc.Context.SendActivityAsync($"BeginDialog {Id}:{generator.ResourceId}");
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }

            public async override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
            {
                var generator = (ResourceMultiLanguageGenerator)dc.Services.Get<LanguageGenerator>();
                Assert.Equal(ResourceId, generator.ResourceId);
                await dc.Context.SendActivityAsync($"ContinueDialog {Id}:{generator.ResourceId}");
                return await dc.EndDialogAsync();
            }
        }

        [Fact]
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

        [Fact]
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

                var result = await lg.GenerateAsync(dialogContext, "This is ${test.name}", new
                {
                    test = new
                    {
                        name = "Tom"
                    }
                });

                await dialogContext.Context.SendActivityAsync(result.ToString());

                return await dialogContext.EndDialogAsync();
            }
        }

        [Fact]
        public async Task TestLGInjection()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);
            DialogManager dm = new DialogManager()
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration("inject.lg");
            dm.RootDialog = (AdaptiveDialog)resourceExplorer.LoadType<Dialog>("inject.dialog");

            await CreateFlow(async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            })
            .Send("hello")
                .AssertReply("[{\"Id\":0,\"Topic\":\"car\"},{\"Id\":1,\"Topic\":\"washing\"},{\"Id\":2,\"Topic\":\"food\"},{\"Id\":3,\"Topic\":\"laundry\"}]")
                .AssertReply("This is an injected message")
                .AssertReply("Hi Jonathan")
                .AssertReply("Jonathan : 2003-03-20")
                .AssertReply("Jonathan, your tasks: car, washing, food and laundry")
                .AssertReply("2")
            .StartTestAsync();
        }

        [Fact]
        public async Task TestLocaleInExpression()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);
            DialogManager dm = new DialogManager()
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration("test.lg");
            dm.RootDialog = (AdaptiveDialog)resourceExplorer.LoadType<Dialog>("locale.dialog");
            await CreateFlow(async (turnContext, cancellationToken) =>
            {
                (turnContext as TurnContext).Locale = "de-DE";
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            })
            .Send("hola")
            .AssertReply("1,122")
            .AssertReply("1,1235")
            .AssertReply("Samstag, 6. Januar 2018")
            .AssertReply("3,14159")
            .StartTestAsync();
        }

        [Fact]
        public async Task TestDateTimeFunctions()
        {
            var resourceExplorer = new ResourceExplorer().LoadProject(GetProjectFolder(), monitorChanges: false);
            DialogManager dm = new DialogManager()
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration("test.lg");
            dm.RootDialog = (AdaptiveDialog)resourceExplorer.LoadType<Dialog>("datetime.dialog");

            await CreateFlow(async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            })
            .Send("hello")
                .AssertReply("2018-01-01T08:02:00.000Z")
                .AssertReply("2018-01-01T08:03:00.000Z")
                .AssertReply("2018-01-06T08:00:00.000Z")
                .AssertReply("2018-01-01T15:00:00.000Z")
                .AssertReply("2018-01-01T08:33:00.000Z")
                .AssertReply("1")
                .AssertReply("1")
                .AssertReply("1")
                .AssertReply("1")
                .AssertReply("1/01/2018")
                .AssertReply("2018")
                .AssertReply("2018-01-01T08:00:00.000Z")
                .AssertReply("2017-01-01T08:00:00.000Z")
                .AssertReply("morning")
                .AssertReply("tomorrow")
                .AssertReply("01-01-2018")
                .AssertReply("2018-01-01T16:00:00.000Z")
                .AssertReply("2018-01-20T00:00:00.000Z")
                .AssertReply("2018-01-20T08:00:00.000Z")
                .AssertReply("2018-01-01T00:00:00.000Z")
                .AssertReply("636503904000000000")
                .AssertReply("True")
            .StartTestAsync();
        }

        [Fact]
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

            var adapter = new TestAdapter(TestAdapter.CreateConversation(MethodBase.GetCurrentMethod().ToString()));
            adapter
                .UseStorage(storage)
                .UseBotState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            return new TestFlow(adapter, handler);
        }

        private TestFlow CreateNoResourceExplorerFlow(BotCallbackHandler handler)
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(MethodBase.GetCurrentMethod().ToString()));
            adapter
                .UseStorage(storage)
                .UseBotState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            return new TestFlow(adapter, handler);
        }
    }

    public class MockLanguageGenerator : LanguageGenerator
    {
        public override Task<object> GenerateAsync(DialogContext dialogContext, string template, object data, CancellationToken cancellationToken = default)
        {
            return Task.FromResult((object)template);
        }
    }
}
