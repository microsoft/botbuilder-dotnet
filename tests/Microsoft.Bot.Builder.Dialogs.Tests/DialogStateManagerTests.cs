#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1201 // Elements should appear in the correct order
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers;
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

            foreach (var key in state.Keys)
            {
                Assert.AreEqual(state.GetValue<object>(key), state[key]);
            }
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

            // test HASH
            state.SetValue($"turn.recognized.intents.test", "intent1");
            state.SetValue($"#test2", "intent2");

            Assert.AreEqual("intent1", state.GetValue<string>("turn.recognized.intents.test"));
            Assert.AreEqual("intent1", state.GetValue<string>("#test"));
            Assert.AreEqual("intent2", state.GetValue<string>("turn.recognized.intents.test2"));
            Assert.AreEqual("intent2", state.GetValue<string>("#test2"));
        }

        [TestMethod]
        public void TestEntityResolvers()
        {
            var dialogs = new DialogSet();
            var dc = new DialogContext(dialogs, new TurnContext(new TestAdapter(), new Schema.Activity()), (DialogState)new DialogState());
            DialogStateManager state = new DialogStateManager(dc);

            // test @ and @@
            var testEntities = new string[] { "entity1", "entity2" };
            var testEntities2 = new string[] { "entity3", "entity4" };
            state.SetValue($"turn.recognized.entities.test", testEntities);
            state.SetValue($"@@test2", testEntities2);

            Assert.AreEqual(testEntities.First(), state.GetValue<string>("turn.recognized.entities.test[0]"));
            Assert.AreEqual(testEntities.First(), state.GetValue<string>("@test"));
            Assert.IsTrue(testEntities.SequenceEqual(state.GetValue<string[]>("turn.recognized.entities.test")));
            Assert.IsTrue(testEntities.SequenceEqual(state.GetValue<string[]>("@@test")));

            Assert.AreEqual(testEntities2.First(), state.GetValue<string>("turn.recognized.entities.test2[0]"));
            Assert.AreEqual(testEntities2.First(), state.GetValue<string>("@test2"));
            Assert.IsTrue(testEntities2.SequenceEqual(state.GetValue<string[]>("turn.recognized.entities.test2")));
            Assert.IsTrue(testEntities2.SequenceEqual(state.GetValue<string[]>("@@test2")));
        }

        public class D2Dialog : Dialog
        {
            public D2Dialog()
                : base("d2")
            {
            }

            public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                dc.State.SetValue($"dialog.options", options);
                dc.State.SetValue($"$bbb", "bbb");
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("$bbb"));
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("dialog.options.test"));
                return await dc.EndDialogAsync(dc.State.GetValue<string>("$bbb"));
            }
        }

        public class D1Dialog : ComponentDialog, IDialogDependencies
        {
            public D1Dialog()
                : base("d1")
            {
                this.AddDialog(new D2Dialog());
            }

            public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                dc.State.SetValue("dialog.xyz", "dialog");
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("dialog.xyz"));
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("$xyz"));
                dc.State.SetValue("$aaa", "dialog2");
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("dialog.aaa"));
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("$aaa"));
                return await dc.BeginDialogAsync("d2", options: new { test = "123" });
            }

            public IEnumerable<Dialog> GetDependencies()
            {
                return _dialogs.GetDialogs();
            }

            public async override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
            {
                dc.State.SetValue("$xyz", result);
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("$xyz"));
                return await dc.EndDialogAsync(result);
            }
        }

        [TestMethod]
        public async Task TestDollarScope()
        {
            await CreateFlow(new D1Dialog())
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

        public class DialogCommandScopeDialog : ComponentDialog
        {
            public DialogCommandScopeDialog()
                : base(nameof(DialogCommandScopeDialog))
            {
            }

            public override Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default)
            {
                return base.BeginDialogAsync(outerDc, options, cancellationToken);
            }
        }

        public class NestedContainerDialog2 : ComponentDialog
        {
            public NestedContainerDialog2()
                : base("d2")
            {
            }

            public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default)
            {
                outerDc.State.SetValue("$name", "d2");
                var name = outerDc.State.GetValue<string>("$name");
                await outerDc.Context.SendActivityAsync($"nested {name}");
                return await outerDc.EndDialogAsync(this.Id);
            }
        }

        public class NestedContainerDialog1 : ComponentDialog
        {
            public NestedContainerDialog1()
                : base("d1")
            {
            }

            public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                dc.State.SetValue("$name", "d1");
                var name = dc.State.GetValue<string>("$name");
                await dc.Context.SendActivityAsync($"nested {name}");
                name = dc.State.GetValue<string>("dialog.name");
                await dc.Context.SendActivityAsync($"nested {name}");
                return await dc.EndDialogAsync(this.Id);
            }
        }

        public class NestedContainerDialog : ComponentDialog, IDialogDependencies
        {
            public NestedContainerDialog()
                : base(nameof(NestedContainerDialog))
            {
                AddDialog(new NestedContainerDialog1());
                AddDialog(new NestedContainerDialog2());
            }

            public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                dc.State.SetValue("$name", "testDialog");
                var name = dc.State.GetValue<String>("$name");
                await dc.Context.SendActivityAsync(name);
                name = dc.State.GetValue<String>("dialog.name");
                await dc.Context.SendActivityAsync(name);
                return await dc.BeginDialogAsync("d1");
            }

            public IEnumerable<Dialog> GetDependencies()
            {
                return _dialogs.GetDialogs();
            }

            public async override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
            {
                if ((string)result == "d2")
                {
                    return await dc.EndDialogAsync();
                }

                var name = dc.State.GetValue<string>("$name");
                await dc.Context.SendActivityAsync(name);
                name = dc.State.GetValue<string>("dialog.name");
                await dc.Context.SendActivityAsync(name);
                return await dc.BeginDialogAsync("d2");
            }
        }

        [TestMethod]
        public async Task TestNestedContainerDialogs()
        {
            await CreateFlow(new NestedContainerDialog())
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

            state.SetValue($"turn.x.y.z", null);
            Assert.AreEqual(null, state.GetValue<object>("turn.x.y.z"));
        }

        private TestFlow CreateFlow(Dialog dialog, ConversationState convoState = null, UserState userState = null, bool sendTrace = false)
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName), sendTrace)
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

