#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers;
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

        public TestFlow CreateDialogContext(Func<DialogContext, CancellationToken, Task> handler)
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(new MemoryStorage())
                .UseBotState(new UserState(new MemoryStorage()))
                .UseBotState(new ConversationState(new MemoryStorage()));

            DialogManager dm = new DialogManager(new LamdaDialog(handler));
            dm.TurnState.Set<ResourceExplorer>(new ResourceExplorer());
            return new TestFlow(adapter, dm.OnTurnAsync).SendConversationUpdate();
        }

        [TestMethod]
        public async Task TestMemoryScopeNullChecks()
        {
            await CreateDialogContext(async (context, ct) =>
            {
                foreach (var memoryScope in context.State.Configuration.MemoryScopes)
                {
                    try
                    {
                        memoryScope.GetMemory(null);
                        Assert.Fail($"Should have thrown exception with null for {memoryScope.Name}");
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        memoryScope.SetMemory(null, new object());
                        Assert.Fail($"Should have thrown exception with null dc for SetMemory {memoryScope.Name}");
                    }
                    catch (Exception)
                    {
                    }
                }
            })
            .StartTestAsync();
        }

        [TestMethod]
        public void TestPathResolverNullChecks()
        {
            var ac = new DialogsComponentRegistration();

            foreach (var resolver in ac.GetPathResolvers())
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
        public async Task TestMemorySnapshot()
        {
            await CreateDialogContext(async (context, ct) =>
            {
                JObject snapshot = context.State.GetMemorySnapshot();
                var dsm = new DialogStateManager(context);
                foreach (var memoryScope in dsm.Configuration.MemoryScopes)
                {
                    if (memoryScope.IncludeInSnapshot)
                    {
                        Assert.IsNotNull(snapshot.Property(memoryScope.Name));
                    }
                    else
                    {
                        Assert.IsNull(snapshot.Property(memoryScope.Name));
                    }
                }
            })
            .StartTestAsync();
        }

        [TestMethod]
        public void TestPathResolverTransform()
        {
            // dollar tests
            Assert.AreEqual("$", new DollarPathResolver().TransformPath("$"));
            Assert.AreEqual("$23", new DollarPathResolver().TransformPath("$23"));
            Assert.AreEqual("$$", new DollarPathResolver().TransformPath("$$"));
            Assert.AreEqual("dialog.foo", new DollarPathResolver().TransformPath("$foo"));
            Assert.AreEqual("dialog.foo.bar", new DollarPathResolver().TransformPath("$foo.bar"));
            Assert.AreEqual("dialog.foo.bar[0]", new DollarPathResolver().TransformPath("$foo.bar[0]"));

            // hash tests
            Assert.AreEqual("#", new HashPathResolver().TransformPath("#"));
            Assert.AreEqual("#23", new HashPathResolver().TransformPath("#23"));
            Assert.AreEqual("##", new HashPathResolver().TransformPath("##"));
            Assert.AreEqual("turn.recognized.intents.foo", new HashPathResolver().TransformPath("#foo"));
            Assert.AreEqual("turn.recognized.intents.foo.bar", new HashPathResolver().TransformPath("#foo.bar"));
            Assert.AreEqual("turn.recognized.intents.foo.bar[0]", new HashPathResolver().TransformPath("#foo.bar[0]"));

            // @ test
            Assert.AreEqual("@", new AtPathResolver().TransformPath("@"));
            Assert.AreEqual("@23", new AtPathResolver().TransformPath("@23"));
            Assert.AreEqual("@@foo", new AtPathResolver().TransformPath("@@foo"));
            Assert.AreEqual("turn.recognized.entities.foo.first()", new AtPathResolver().TransformPath("@foo"));
            Assert.AreEqual("turn.recognized.entities.foo.first().bar", new AtPathResolver().TransformPath("@foo.bar"));

            // @@ teest
            Assert.AreEqual("@@", new AtAtPathResolver().TransformPath("@@"));
            Assert.AreEqual("@@23", new AtAtPathResolver().TransformPath("@@23"));
            Assert.AreEqual("@@@@", new AtAtPathResolver().TransformPath("@@@@"));
            Assert.AreEqual("turn.recognized.entities.foo", new AtAtPathResolver().TransformPath("@@foo"));

            // % config tests
            Assert.AreEqual("%", new PercentPathResolver().TransformPath("%"));
            Assert.AreEqual("%23", new PercentPathResolver().TransformPath("%23"));
            Assert.AreEqual("%%", new PercentPathResolver().TransformPath("%%"));
            Assert.AreEqual("class.foo", new PercentPathResolver().TransformPath("%foo"));
            Assert.AreEqual("class.foo.bar", new PercentPathResolver().TransformPath("%foo.bar"));
            Assert.AreEqual("class.foo.bar[0]", new PercentPathResolver().TransformPath("%foo.bar[0]"));
        }

        [TestMethod]
        public async Task TestSimpleValues()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                // simple value types
                dc.State.SetValue("UseR.nuM", 15);
                dc.State.SetValue("uSeR.NuM", 25);
                Assert.AreEqual(25, dc.State.GetValue<int>("user.num"));

                dc.State.SetValue("UsEr.StR", "string1");
                dc.State.SetValue("usER.STr", "string2");
                Assert.AreEqual("string2", dc.State.GetValue<string>("USer.str"));

                // simple value types
                dc.State.SetValue("ConVErsation.nuM", 15);
                dc.State.SetValue("ConVErSation.NuM", 25);
                Assert.AreEqual(25, dc.State.GetValue<int>("conversation.num"));

                dc.State.SetValue("ConVErsation.StR", "string1");
                dc.State.SetValue("CoNVerSation.STr", "string2");
                Assert.AreEqual("string2", dc.State.GetValue<string>("conversation.str"));

                // simple value types
                dc.State.SetValue("tUrn.nuM", 15);
                dc.State.SetValue("turN.NuM", 25);
                Assert.AreEqual(25, dc.State.GetValue<int>("turn.num"));

                dc.State.SetValue("tuRn.StR", "string1");
                dc.State.SetValue("TuRn.STr", "string2");
                Assert.AreEqual("string2", dc.State.GetValue<string>("turn.str"));
            }).StartTestAsync();
        }

        [TestMethod]
        public async Task TestEntitiesRetrieval()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                var array = new JArray();
                array.Add("test1");
                array.Add("test2");
                array.Add("test3");

                var array2 = new JArray();
                array2.Add("testx");
                array2.Add("testy");
                array2.Add("testz");

                var arrayarray = new JArray();
                arrayarray.Add(array2);
                arrayarray.Add(array);

                dc.State.SetValue("turn.recognized.entities.single", array);
                dc.State.SetValue("turn.recognized.entities.double", arrayarray);

                Assert.AreEqual("test1", dc.State.GetValue<string>("@single"));
                Assert.AreEqual("testx", dc.State.GetValue<string>("@double"));
                Assert.AreEqual("test1", dc.State.GetValue<string>("turn.recognized.entities.single.First()"));
                Assert.AreEqual("testx", dc.State.GetValue<string>("turn.recognized.entities.double.First()"));

                arrayarray = new JArray();
                array = new JArray();
                array.Add(JObject.Parse("{'name':'test1'}"));
                array.Add(JObject.Parse("{'name':'test2'}"));
                array.Add(JObject.Parse("{'name':'test2'}"));

                array2 = new JArray();
                array2.Add(JObject.Parse("{'name':'testx'}"));
                array2.Add(JObject.Parse("{'name':'testy'}"));
                array2.Add(JObject.Parse("{'name':'testz'}"));
                arrayarray.Add(array2);
                arrayarray.Add(array);
                dc.State.SetValue("turn.recognized.entities.single", array);
                dc.State.SetValue("turn.recognized.entities.double", arrayarray);

                Assert.AreEqual("test1", dc.State.GetValue<string>("@single.name"));
                Assert.AreEqual("testx", dc.State.GetValue<string>("@double.name"));
                Assert.AreEqual("test1", dc.State.GetValue<string>("turn.recognized.entities.single.First().name"));
                Assert.AreEqual("testx", dc.State.GetValue<string>("turn.recognized.entities.double.First().name"));
            }).StartTestAsync();
        }

        [TestMethod]
        public async Task TestComplexValuePaths()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                // complex type paths
                dc.State.SetValue("UseR.fOo", foo);
                Assert.AreEqual("bob", dc.State.GetValue<string>("user.foo.SuBname.name"));

                // complex type paths
                dc.State.SetValue("ConVerSation.FOo", foo);
                Assert.AreEqual("bob", dc.State.GetValue<string>("conversation.foo.SuBname.name"));

                // complex type paths
                dc.State.SetValue("TurN.fOo", foo);
                Assert.AreEqual("bob", dc.State.GetValue<string>("TuRN.foo.SuBname.name"));
            }).StartTestAsync();
        }

        [TestMethod]
        public async Task TestComplexPathExpressions()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                // complex type paths
                dc.State.SetValue("user.name", "joe");
                dc.State.SetValue("conversation.stuff[user.name]", "test");
                dc.State.SetValue("conversation.stuff['frank']", "test2");
                dc.State.SetValue("conversation.stuff[\"susan\"]", "test3");
                dc.State.SetValue("conversation.stuff['Jo.Bob']", "test4");
                Assert.AreEqual("test", dc.State.GetValue<string>("conversation.stuff.joe"), "complex set should set");
                Assert.AreEqual("test", dc.State.GetValue<string>("conversation.stuff[user.name]"), "complex get should get");
                Assert.AreEqual("test2", dc.State.GetValue<string>("conversation.stuff['frank']"), "complex get should get");
                Assert.AreEqual("test3", dc.State.GetValue<string>("conversation.stuff[\"susan\"]"), "complex get should get");
                Assert.AreEqual("test4", dc.State.GetValue<string>("conversation.stuff[\"Jo.Bob\"]"), "complex get should get");
            }).StartTestAsync();
        }

        [TestMethod]
        public async Task TestGetValue()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                // complex type paths
                dc.State.SetValue("user.name.first", "joe");
                Assert.AreEqual("joe", dc.State.GetValue<string>("user.name.first"));

                Assert.AreEqual(null, dc.State.GetValue<string>("user.xxx"));
                Assert.AreEqual("default", dc.State.GetValue<string>("user.xxx", () => "default"));

                foreach (var key in dc.State.Keys)
                {
                    Assert.AreEqual(dc.State.GetValue<object>(key), dc.State[key]);
                }
            }).StartTestAsync();
        }

        [TestMethod]
        public async Task TestGetValueT()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                // complex type paths
                dc.State.SetValue("UseR.fOo", foo);
                Assert.AreEqual(dc.State.GetValue<Foo>("user.foo").SubName.Name, "bob");

                // complex type paths
                dc.State.SetValue("ConVerSation.FOo", foo);
                Assert.AreEqual(dc.State.GetValue<Foo>("conversation.foo").SubName.Name, "bob");

                // complex type paths
                dc.State.SetValue("TurN.fOo", foo);
                Assert.AreEqual(dc.State.GetValue<Foo>("turn.foo").SubName.Name, "bob");
            }).StartTestAsync();
        }

        [TestMethod]
        public async Task TestSetValue_RootScope()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                try
                {
                    dc.State.SetValue(null, 13);
                    Assert.Fail("Should have thrown with null memory scope");
                }
                catch (ArgumentNullException err)
                {
                    Assert.IsTrue(err.Message.Contains("path"));
                }

                try
                {
                    // complex type paths
                    dc.State.SetValue("xxx", 13);
                    Assert.Fail("Should have thrown with unknown memory scope");
                }
                catch (ArgumentOutOfRangeException err)
                {
                    Assert.IsTrue(err.Message.Contains("does not match memory scope"));
                }
            }).StartTestAsync();
        }

        [TestMethod]
        public async Task TestRemoveValue_RootScope()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                try
                {
                    dc.State.RemoveValue(null);
                    Assert.Fail("Should have thrown with null memory scope");
                }
                catch (ArgumentNullException err)
                {
                    Assert.IsTrue(err.Message.Contains("path"));
                }

                try
                {
                    dc.State.RemoveValue("user");
                    Assert.Fail("Should have thrown with known root memory scope");
                }
                catch (NotSupportedException)
                {
                }

                try
                {
                    dc.State.RemoveValue("xxx");
                    Assert.Fail("Should have thrown with unknown memory scope");
                }
                catch (NotSupportedException)
                {
                }
            }).StartTestAsync();
        }

        [TestMethod]
        public async Task TestHashResolver()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                // test HASH
                dc.State.SetValue($"turn.recognized.intents.test", "intent1");
                dc.State.SetValue($"#test2", "intent2");

                Assert.AreEqual("intent1", dc.State.GetValue<string>("turn.recognized.intents.test"));
                Assert.AreEqual("intent1", dc.State.GetValue<string>("#test"));
                Assert.AreEqual("intent2", dc.State.GetValue<string>("turn.recognized.intents.test2"));
                Assert.AreEqual("intent2", dc.State.GetValue<string>("#test2"));
            }).StartTestAsync();
        }

        [TestMethod]
        public async Task TestEntityResolvers()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                // test @ and @@
                var testEntities = new string[] { "entity1", "entity2" };
                var testEntities2 = new string[] { "entity3", "entity4" };
                dc.State.SetValue($"turn.recognized.entities.test", testEntities);
                dc.State.SetValue($"@@test2", testEntities2);

                Assert.AreEqual(testEntities.First(), dc.State.GetValue<string>("turn.recognized.entities.test[0]"));
                Assert.AreEqual(testEntities.First(), dc.State.GetValue<string>("@test"));
                Assert.IsTrue(testEntities.SequenceEqual(dc.State.GetValue<string[]>("turn.recognized.entities.test")));
                Assert.IsTrue(testEntities.SequenceEqual(dc.State.GetValue<string[]>("@@test")));

                Assert.AreEqual(testEntities2.First(), dc.State.GetValue<string>("turn.recognized.entities.test2[0]"));
                Assert.AreEqual(testEntities2.First(), dc.State.GetValue<string>("@test2"));
                Assert.IsTrue(testEntities2.SequenceEqual(dc.State.GetValue<string[]>("turn.recognized.entities.test2")));
                Assert.IsTrue(testEntities2.SequenceEqual(dc.State.GetValue<string[]>("@@test2")));
            }).StartTestAsync();
        }

        public class D2Dialog : Dialog
        {
            public D2Dialog()
                : base("d2")
            {
            }

            public int MaxValue { get; set; } = 20;

            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                dc.State.SetValue($"dialog.options", options);
                dc.State.SetValue($"$bbb", "bbb");
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("$bbb"));
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("dialog.options.test"));
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("%MaxValue"));
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

            public int MaxValue { get; set; } = 10;

            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                dc.State.SetValue("dialog.xyz", "dialog");
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("dialog.xyz"));
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("$xyz"));
                dc.State.SetValue("$aaa", "dialog2");
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("dialog.aaa"));
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("$aaa"));
                await dc.Context.SendActivityAsync(dc.State.GetValue<string>("%MaxValue"));
                return await dc.BeginDialogAsync("d2", options: new { test = "123" });
            }

            public IEnumerable<Dialog> GetDependencies()
            {
                return this.Dialogs.GetDialogs();
            }

            public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
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
                        .AssertReply("10")

                        // d2
                        .AssertReply("bbb")
                        .AssertReply("123")
                        .AssertReply("20")
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

            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default)
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

            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
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

            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                dc.State.SetValue("$name", "testDialog");
                var name = dc.State.GetValue<string>("$name");
                await dc.Context.SendActivityAsync(name);
                name = dc.State.GetValue<string>("dialog.name");
                await dc.Context.SendActivityAsync(name);
                return await dc.BeginDialogAsync("d1");
            }

            public IEnumerable<Dialog> GetDependencies()
            {
                return Dialogs.GetDialogs();
            }

            public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
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
        public async Task TestExpressionSet()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                dc.State.SetValue($"turn.x.y.z", null);
                Assert.AreEqual(null, dc.State.GetValue<object>("turn.x.y.z"));
            }).StartTestAsync();
        }

        internal class TestDialog : Dialog
        {
            public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                var data = dc.State.GetValue<string>("conversation.test", () => "unknown");
                dc.Context.SendActivityAsync(data);
                dc.State.SetValue("conversation.test", "havedata");
                return Task.FromResult(new DialogTurnResult(DialogTurnStatus.Waiting));
            }

            public override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
            {
                switch (dc.Context.Activity.Text)
                {
                    case "throw":
                        throw new Exception("throwing");
                    case "end":
                        return dc.EndDialogAsync();
                }

                var data = dc.State.GetValue<string>("conversation.test", () => "unknown");
                dc.Context.SendActivityAsync(data);
                return Task.FromResult(new DialogTurnResult(DialogTurnStatus.Waiting));
            }
        }

        [TestMethod]
        public async Task TestConversationResetOnException()
        {
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .UseStorage(storage)
                .UseBotState(userState, conversationState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            adapter.OnTurnError = async (context, exception) =>
            {
                await conversationState.DeleteAsync(context);
                await context.SendActivityAsync(exception.Message);
            };

            var dm = new DialogManager(new TestDialog());

            await new TestFlow((TestAdapter)adapter, (turnContext, cancellationToken) =>
            {
                return dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken);
            })
            .Send("yo1")
                .AssertReply("unknown")
            .Send("yo2")
                .AssertReply("havedata")
            .Send("throw")
                .AssertReply("throwing")
            .Send("yo3")
                .AssertReply("unknown")
            .Send("yo4")
                .AssertReply("havedata")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestConversationResetOnExpiration()
        {
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .UseStorage(storage)
                .UseBotState(userState, conversationState);

            adapter.OnTurnError = async (context, exception) =>
            {
                await conversationState.DeleteAsync(context);
                await context.SendActivityAsync(exception.Message);
            };

            var dm = new DialogManager(new TestDialog())
            {
                ExpireAfter = 1000
            };

            await new TestFlow((TestAdapter)adapter, (turnContext, cancellationToken) =>
            {
                return dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken);
            })
            .Send("yo")
                .AssertReply("unknown")
            .Send("yo")
                .AssertReply("havedata")
            .Delay(TimeSpan.FromSeconds(1.1))
            .Send("yo")
                .AssertReply("unknown", "Should have expired conversation and ended up with yo=>unknown")
            .Send("yo")
                .AssertReply("havedata")
            .Send("yo")
                .AssertReply("havedata")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestChangeTracking()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                var state = dc.State;
                var dialogPaths = state.TrackPaths(new List<string> { "dialog.user.first", "dialog.user.last" });

                state.SetValue("dialog.eventCounter", 0);
                Assert.IsFalse(state.AnyPathChanged(0, dialogPaths));

                state.SetValue("dialog.eventCounter", 1);
                state.SetValue("dialog.foo", 3);
                Assert.IsFalse(state.AnyPathChanged(0, dialogPaths));

                state.SetValue("dialog.eventCounter", 2);
                state.SetValue("dialog.user.first", "bart");
                Assert.IsTrue(state.AnyPathChanged(1, dialogPaths));

                state.SetValue("dialog.eventCounter", 3);
                state.SetValue("dialog.user", new Dictionary<string, object> { { "first", "tom" }, { "last", "starr" } });
                Assert.IsTrue(state.AnyPathChanged(2, dialogPaths));

                state.SetValue("dialog.eventCounter", 4);
                Assert.IsFalse(state.AnyPathChanged(3, dialogPaths));
            }).StartTestAsync();
        }

        private TestFlow CreateFlow(Dialog dialog, ConversationState convoState = null, UserState userState = null, bool sendTrace = false)
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName), sendTrace)
                .UseStorage(new MemoryStorage())
                .UseBotState(new UserState(new MemoryStorage()))
                .UseBotState(convoState ?? new ConversationState(new MemoryStorage()))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

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
