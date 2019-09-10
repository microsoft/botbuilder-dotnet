#pragma warning disable SA1402 // File may only contain a single type
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Events;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
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
            var dialogs = new DialogSet();
            var dc = new DialogContext(dialogs, new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            DialogStateManager state = new DialogStateManager(dc);

            foreach (var resolver in DialogStateManager.PathResolvers)
            {
                try
                {
                    resolver.Matches(null);
                    Assert.Fail($"Should have thrown exception with null for matches() {resolver.GetType().Name}");
                }
                catch (ArgumentNullException)
                {
                }

                try
                {
                    resolver.TryGetValue(null, "test", out object value);
                    Assert.Fail($"Should have thrown exception with null dc for TryGetValue() {resolver.GetType().Name}");
                }
                catch (ArgumentNullException)
                {
                }

                try
                {
                    resolver.TryGetValue(dc, null, out object value);
                    Assert.Fail($"Should have thrown exception with null path for TryGetValue() {resolver.GetType().Name}");
                }
                catch (ArgumentNullException)
                {
                }

                try
                {
                    resolver.RemoveValue(null, null);
                    Assert.Fail($"Should have thrown exception with null dc for RemovePath() {resolver.GetType().Name}");
                }
                catch (ArgumentNullException)
                {
                }

                try
                {
                    resolver.RemoveValue(dc, null);
                    Assert.Fail($"Should have thrown exception with null path for RemovePath() {resolver.GetType().Name}");
                }
                catch (ArgumentNullException)
                {
                }
            }
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
        public async Task TestDialogResolvers()
        {
            var d2 = new AdaptiveDialog("d2")
            {
                Events = new List<IOnEvent>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty() { Property = "$bbb", Value = "'bbb'" },
                            new SendActivity("{$bbb}"),
                            new SendActivity("{dialog.options.test}"),
                            new SendActivity("{%test}"),
                        }
                    }
                }
            };

            var testDialog = new AdaptiveDialog("testDialog")
            {
                AutoEndDialog = false,
                Events = new List<IOnEvent>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty() { Property = "dialog.xyz", Value = "'dialog'" },
                            new SendActivity("{dialog.xyz}"),
                            new SendActivity("{$xyz}"),
                            new SetProperty() { Property = "$aaa", Value = "'d1'" },
                            new SendActivity("{dialog.aaa}"),
                            new SendActivity("{$aaa}"),
                            new SetProperty() { Property = "$aaa", Value = "'d1-test'" },
                            new SendActivity("{dialog.aaa}"),
                            new SendActivity("{$aaa}"),
                            new BeginDialog(d2.Id)
                            {
                                Options = new { test = "123" }
                            }
                        }
                    }
                }
            };

            testDialog.AddDialog(d2);

            await CreateFlow(testDialog)
                    .SendConversationUpdate()

                        // d1
                        .AssertReply("dialog")
                        .AssertReply("dialog")
                        .AssertReply("d1")
                        .AssertReply("d1")
                        .AssertReply("d1-test")
                        .AssertReply("d1-test")

                        // d2
                        .AssertReply("bbb")
                        .AssertReply("123")
                        .AssertReply("123")
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
                Events = new List<IOnEvent>()
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
                Events = new List<IOnEvent>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Property = "dialog.name",
                                Value = "'testDialog'"
                            },
                            new SendActivity("{dialog.name}"),
                            new IfCondition()
                            {
                                Condition = "dialog.name == 'testDialog'",
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("nested dialogCommand {dialog.name}")
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
                Events = new List<IOnEvent>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("nested d2 {$name}"),
                            new SetProperty() { Property = "$name", Value = "'testDialogd2'" },
                            new SendActivity("nested d2 {$name}"),
                        }
                    }
                }
            };

            var testDialog = new AdaptiveDialog("testDialog")
            {
                AutoEndDialog = false,

                Events = new List<IOnEvent>()
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
                                Events = new List<IOnEvent>()
                                {
                                    new OnBeginDialog()
                                    {
                                        Actions = new List<Dialog>()
                                        {
                                            new SendActivity("nested {$name}"),
                                            //new SetProperty() { Property = "$name", Value = "'$name'" },
                                            //new SendActivity("nested {$name}"),
                                            //new SendActivity("nested {dialog.name}"),
                                            //new SetProperty() { Property = "dialog.name", Value = "'dialog.name'" },
                                            //new SendActivity("nested {dialog.name}"),
                                        }
                                    }
                                }
                            },
                            new SendActivity("{$name}"),
                            new SendActivity("{dialog.name}"),
                            // new BeginDialog(d2.Id)
                        }
                    }
                }
            };

            testDialog.AddDialog(d2);

            await CreateFlow(testDialog)
                    .SendConversationUpdate()
                        .AssertReply("testDialog")
                        .AssertReply("testDialog")
                        .AssertReply("nested testDialog")
                        .AssertReply("nested $name")
                        .AssertReply("nested $name")
                        .AssertReply("nested dialog.name")
                        .AssertReply("nested dialog.name")
                        .AssertReply("testDialog")
                        .AssertReply("testDialog")
                    // .AssertReply("nested d2 testDialog")
                    // .AssertReply("nested d2 testDialogd2")
                    .StartTestAsync();
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

