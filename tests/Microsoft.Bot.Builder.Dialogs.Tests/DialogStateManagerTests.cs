#pragma warning disable SA1402 // File may only contain a single type
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class DialogStateManagerTests
    {
        private Foo foo = new Foo()
        {
            Name = "Tom",
            Age = 15,
            Cool = true,
            SubName = new Bar()
            {
                Name = "bob",
                Age = 122,
                Cool = false
            }
        };

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestMemoryScopeNullChecks()
        {
            var dialogs = new DialogSet();
            var dc = new DialogContext(dialogs, new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            DialogStateManager state = new DialogStateManager(dc);

            foreach (var memoryScope in DialogStateManager.MemoryScopes)
            {
                try
                {
                    memoryScope.GetMemory(null);
                    Assert.Fail($"Should have thrown exception with null for {memoryScope.Name}");
                }
                catch (ArgumentNullException)
                {
                }

                try
                {
                    memoryScope.SetMemory(null, new object());
                    Assert.Fail($"Should have thrown exception with null dc for SetMemory {memoryScope.Name}");
                }
                catch (ArgumentNullException)
                {
                }

                try
                {
                    memoryScope.SetMemory(dc, null);
                    Assert.Fail($"Should have thrown exception with null memory for SetMemory {memoryScope.Name}");
                }
                catch (ArgumentNullException)
                {
                }
            }
        }

        [TestMethod]
        public void TestPathResolverNullChecks()
        {
            foreach (var resolver in DialogStateManager.PathResolvers)
            {
                try
                {
                    resolver.TransformPath(null);
                    Assert.Fail($"Should have thrown exception with null for matches() {resolver.GetType().Name}");
                }
                catch (ArgumentNullException)
                {
                }
            }
        }

        [TestMethod]
        public void TestPathResolverTransform()
        {
            // dollar tests
            Assert.AreEqual("dialog", new DollarPathResolver().TransformPath("$"));
            Assert.AreEqual("dialog.foo", new DollarPathResolver().TransformPath("$foo"));
            Assert.AreEqual("dialog.foo.bar", new DollarPathResolver().TransformPath("$foo.bar"));
            Assert.AreEqual("dialog.foo.bar[0]", new DollarPathResolver().TransformPath("$foo.bar[0]"));

            // hash tests
            Assert.AreEqual("turn.recognized.intents", new HashPathResolver().TransformPath("#"));
            Assert.AreEqual("turn.recognized.intents.foo", new HashPathResolver().TransformPath("#foo"));
            Assert.AreEqual("turn.recognized.intents.foo.bar", new HashPathResolver().TransformPath("#foo.bar"));
            Assert.AreEqual("turn.recognized.intents.foo.bar[0]", new HashPathResolver().TransformPath("#foo.bar[0]"));

            // @ test
            Assert.AreEqual("turn.recognized.entities.foo[0]", new AtPathResolver().TransformPath("@foo"));

            // @@ teest
            Assert.AreEqual("turn.recognized.entities.foo", new AtAtPathResolver().TransformPath("@@foo"));
            Assert.AreEqual("turn.recognized.entities", new AtAtPathResolver().TransformPath("@@"));
        }

        [TestMethod]
        public void TestSimpleValues()
        {
            var dialogs = new DialogSet();
            var dc = new DialogContext(dialogs, new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            DialogStateManager state = new DialogStateManager(dc);

            // simple value types
            state.SetValue("UseR.nuM", 15);
            state.SetValue("uSeR.NuM", 25);
            Assert.AreEqual(25, state.GetValue<int>("user.num"));

            state.SetValue("UsEr.StR", "string1");
            state.SetValue("usER.STr", "string2");
            Assert.AreEqual("string2", state.GetValue<string>("USer.str"));

            // simple value types
            state.SetValue("ConVErsation.nuM", 15);
            state.SetValue("ConVErSation.NuM", 25);
            Assert.AreEqual(25, state.GetValue<int>("conversation.num"));

            state.SetValue("ConVErsation.StR", "string1");
            state.SetValue("CoNVerSation.STr", "string2");
            Assert.AreEqual("string2", state.GetValue<string>("conversation.str"));

            // simple value types
            state.SetValue("tUrn.nuM", 15);
            state.SetValue("turN.NuM", 25);
            Assert.AreEqual(25, state.GetValue<int>("turn.num"));

            state.SetValue("tuRn.StR", "string1");
            state.SetValue("TuRn.STr", "string2");
            Assert.AreEqual("string2", state.GetValue<string>("turn.str"));
        }

        [TestMethod]
        public void TestComplexValuePaths()
        {
            var dialogs = new DialogSet();
            var dc = new DialogContext(dialogs, new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            DialogStateManager state = new DialogStateManager(dc);

            // complex type paths
            state.SetValue("UseR.fOo", foo);
            Assert.AreEqual("bob", state.GetValue<string>("user.foo.SuBname.name"));

            // complex type paths
            state.SetValue("ConVerSation.FOo", foo);
            Assert.AreEqual("bob", state.GetValue<string>("conversation.foo.SuBname.name"));

            // complex type paths
            state.SetValue("TurN.fOo", foo);
            Assert.AreEqual("bob", state.GetValue<string>("TuRN.foo.SuBname.name"));
        }

        [TestMethod]
        [Ignore] // NOTE: This needs to be revisited
        public void TestComplexPathExpressions()
        {
            var dialogs = new DialogSet();
            var dc = new DialogContext(dialogs, new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            DialogStateManager state = new DialogStateManager(dc);

            // complex type paths
            state.SetValue("user.name", "joe");
            state.SetValue("conversation.stuff[user.name]", "test");
            var value = state.GetValue<string>("conversation.stuff.joe");
            Assert.AreEqual("test", value, "complex set should set");
            value = state.GetValue<string>("conversation.stuff[user.name]");
            Assert.AreEqual("test", value, "complex get should get");
        }

        [TestMethod]
        public void TestGetValue()
        {
            var dialogs = new DialogSet();
            var dc = new DialogContext(dialogs, new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            DialogStateManager state = new DialogStateManager(dc);

            // complex type paths
            state.SetValue("user.name.first", "joe");
            Assert.AreEqual("joe", state.GetValue<string>("user.name.first"));

            Assert.AreEqual(null, state.GetValue<string>("user.xxx"));
            Assert.AreEqual("default", state.GetValue<string>("user.xxx", () => "default"));
        }

        [TestMethod]
        public void TestGetValueT()
        {
            var dialogs = new DialogSet();
            var dc = new DialogContext(dialogs, new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            DialogStateManager state = new DialogStateManager(dc);

            // complex type paths
            state.SetValue("UseR.fOo", foo);
            Assert.AreEqual(state.GetValue<Foo>("user.foo").SubName.Name, "bob");

            // complex type paths
            state.SetValue("ConVerSation.FOo", foo);
            Assert.AreEqual(state.GetValue<Foo>("conversation.foo").SubName.Name, "bob");

            // complex type paths
            state.SetValue("TurN.fOo", foo);
            Assert.AreEqual(state.GetValue<Foo>("turn.foo").SubName.Name, "bob");
        }

        [TestMethod]
        public void TestHashResolver()
        {
            var dialogs = new DialogSet();
            var dc = new DialogContext(dialogs, new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            DialogStateManager state = new DialogStateManager(dc);
            ExpressionEngine engine = new ExpressionEngine();

            // test HASH
            state.SetValue($"turn.recognized.intents.test", "intent1");
            state.SetValue($"#test2", "intent2");

            Assert.AreEqual("intent1", engine.Parse("turn.recognized.intents.test").TryEvaluate(state).value);
            Assert.AreEqual("intent1", engine.Parse("#test").TryEvaluate(state).value);
            Assert.AreEqual("intent2", engine.Parse("turn.recognized.intents.test2").TryEvaluate(state).value);
            Assert.AreEqual("intent2", engine.Parse("#test2").TryEvaluate(state).value);
        }

        [TestMethod]
        public void TestEntityResolvers()
        {
            var dialogs = new DialogSet();
            var dc = new DialogContext(dialogs, new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            DialogStateManager state = new DialogStateManager(dc);
            ExpressionEngine engine = new ExpressionEngine();

            // test @ and @@
            var testEntities = new string[] { "entity1", "entity2" };
            var testEntities2 = new string[] { "entity3", "entity4" };
            state.SetValue($"turn.recognized.entities.test", testEntities);
            state.SetValue($"@@test2", testEntities2);

            Assert.AreEqual(testEntities.First(), engine.Parse("turn.recognized.entities.test[0]").TryEvaluate(state).value);
            Assert.AreEqual(testEntities.First(), engine.Parse("@test").TryEvaluate(state).value);
            Assert.IsTrue(testEntities.SequenceEqual(((JArray)engine.Parse("turn.recognized.entities.test").TryEvaluate(state).value).ToObject<string[]>()));
            Assert.IsTrue(testEntities.SequenceEqual(((JArray)engine.Parse("@@test").TryEvaluate(state).value).ToObject<string[]>()));

            Assert.AreEqual(testEntities2.First(), engine.Parse("turn.recognized.entities.test2[0]").TryEvaluate(state).value);
            Assert.AreEqual(testEntities2.First(), engine.Parse("@test2").TryEvaluate(state).value);
            Assert.IsTrue(testEntities2.SequenceEqual(((JArray)engine.Parse("turn.recognized.entities.test2").TryEvaluate(state).value).ToObject<string[]>()));
            Assert.IsTrue(testEntities2.SequenceEqual(((JArray)engine.Parse("@@test2").TryEvaluate(state).value).ToObject<string[]>()));
        }

        [TestMethod]
        public async Task TestDollarScope()
        {
            var d2 = new AdaptiveDialog("d2")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty() { Property = "$bbb", Value = "'bbb'" },
                            new SendActivity("{$bbb}"),
                            new SendActivity("{dialog.options.test}"),
                            new EndDialog() { Value = "$bbb" }
                        }
                    }
                }
            };

            var testDialog = new AdaptiveDialog("testDialog")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty() { Property = "dialog.xyz", Value = "'dialog'" },
                            new SendActivity("{dialog.xyz}"),
                            new SendActivity("{$xyz}"),
                            new SetProperty() { Property = "$aaa", Value = "'dialog2'" },
                            new SendActivity("{dialog.aaa}"),
                            new SendActivity("{$aaa}"),
                            new BeginDialog(d2.Id)
                            {
                                Options = new { test = "123" },
                                ResultProperty = "$xyz"
                            },
                            new SendActivity("{$xyz}"),
                        }
                    }
                }
            };

            testDialog.Dialogs.Add(d2);

            await CreateFlow(testDialog)
                    .SendConversationUpdate()

                        // d1
                        .AssertReply("dialog")
                        .AssertReply("dialog")
                        .AssertReply("dialog2")
                        .AssertReply("dialog2")

                        // d2
                        .AssertReply("bbb")
                        .AssertReply("123")
                        .AssertReply("bbb")
                    .StartTestAsync();
        }

        [TestMethod]
        public async Task TestBuiltInTurnProperties()
        {
            var testDialog = new AdaptiveDialog("testDialog")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("IntentNumber1", "intent1"),
                        new IntentPattern("NameIntent", ".*name is (?<name>.*)"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("{turn.activity.text}"),
                        }
                    },
                    new OnIntent(
                        intent: "IntentNumber1",
                        actions: new List<Dialog>()
                        {
                            new SendActivity("{turn.activity.text}"),
                            new SendActivity("{turn.recognized.intent}"),
                            new SendActivity("{turn.recognized.score}"),
                            new SendActivity("{turn.recognized.text}"),
                            new SendActivity("{turn.recognized.intents.intentnumber1.score}"),
                        }),
                    new OnIntent(
                        intent: "NameIntent",
                        actions: new List<Dialog>()
                        {
                            new SendActivity("{turn.recognized.entities.name[0]}"),
                        }),
                }
            };

            await CreateFlow(testDialog)
                .Send("hi")
                    .AssertReply("hi")
                .Send("intent1")
                    .AssertReply("intent1")
                    .AssertReply("IntentNumber1")
                    .AssertReply("1")
                    .AssertReply("intent1")
                    .AssertReply("1")
                .Send("my name is joe")
                    .AssertReply("joe")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TestDialogCommandScope()
        {
            var testDialog = new AdaptiveDialog("testDialog")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Property = "$name",
                                Value = "'testDialog'"
                            },
                            new SendActivity("{$name}"),
                            new IfCondition()
                            {
                                Condition = "$name == 'testDialog'",
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("nested dialogCommand {$name}")
                                }
                            }
                        }
                    }
                }
            };

            await CreateFlow(testDialog)
                    .SendConversationUpdate()
                        .AssertReply("testDialog")
                        .AssertReply("nested dialogCommand testDialog")
                    .StartTestAsync();
        }

        [TestMethod]
        public async Task TestNestedContainerDialogs()
        {
            var d2 = new AdaptiveDialog("d2")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty() { Property = "$name", Value = "'d2'" },
                            new SendActivity("nested {$name}"),
                        }
                    }
                }
            };

            var testDialog = new AdaptiveDialog("testDialog")
            {
                AutoEndDialog = false,

                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty() { Property = "$name", Value = "'testDialog'" },
                            new SendActivity("{$name}"),
                            new SendActivity("{dialog.name}"),
                            new AdaptiveDialog("d1")
                            {
                                Triggers = new List<OnCondition>()
                                {
                                    new OnBeginDialog()
                                    {
                                        Actions = new List<Dialog>()
                                        {
                                            new SetProperty() { Property = "$name", Value = "'d1'" },
                                            new SendActivity("nested {$name}"),
                                            new SendActivity("nested {dialog.name}"),
                                        }
                                    }
                                }
                            },
                            new SendActivity("{$name}"),
                            new SendActivity("{dialog.name}"),
                            new BeginDialog(d2.Id)
                        }
                    }
                }
            };

            testDialog.Dialogs.Add(d2);

            await CreateFlow(testDialog)
                    .SendConversationUpdate()
                        .AssertReply("testDialog")
                        .AssertReply("testDialog")
                        .AssertReply("nested d1")
                        .AssertReply("nested d1")
                        .AssertReply("testDialog")
                        .AssertReply("testDialog")
                        .AssertReply("nested d2")
                    .StartTestAsync();
        }

        [TestMethod]
        public void TestExpressionSet()
        {
            var dialogs = new DialogSet();
            var dc = new DialogContext(dialogs, new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            DialogStateManager state = new DialogStateManager(dc);
            ExpressionEngine engine = new ExpressionEngine();

            state.SetValue($"turn.x.y.z", null);
            Assert.AreEqual(null, engine.Parse("turn.x.y.z").TryEvaluate(state).value);
        }

        private TestFlow CreateFlow(AdaptiveDialog dialog, ConversationState convoState = null, UserState userState = null, bool sendTrace = false)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var resourceExplorer = new ResourceExplorer();

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName), sendTrace)
                .Use(new RegisterClassMiddleware<ResourceExplorer>(resourceExplorer))
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(resourceExplorer)
                .Use(new RegisterClassMiddleware<IStorage>(new MemoryStorage()))
                .Use(new AutoSaveStateMiddleware(userState ?? new UserState(new MemoryStorage()), convoState ?? new ConversationState(new MemoryStorage())))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var dm = new DialogManager(dialog);

            return new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }
    }

    public class Bar
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public bool Cool { get; set; }
    }

    public class Foo
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public bool Cool { get; set; }

        public Bar SubName { get; set; }
    }
}

