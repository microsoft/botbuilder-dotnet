// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable SA1201 // Elements should appear in the correct order

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class MemoryScopeTests
    {
        public TestContext TestContext { get; set; }

        public TestFlow CreateDialogContext(Func<DialogContext, CancellationToken, Task> handler)
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(new MemoryStorage())
                .UseState(new UserState(new MemoryStorage()), new ConversationState(new MemoryStorage()));
            DialogManager dm = new DialogManager(new LamdaDialog(handler));
            return new TestFlow(adapter, (context, ct) =>
            {
                return dm.OnTurnAsync(context, ct);
            }).SendConversationUpdate();
        }

        [TestMethod]
        public async Task SimpleMemoryScopesTest()
        {
            await CreateDialogContext(async (dc, ct) =>
            {
                var dsm = dc.State as DialogStateManager;
                foreach (var memoryScope in dsm.Configuration.MemoryScopes.Where(ms => !(ms is ThisMemoryScope || ms is DialogMemoryScope || ms is ClassMemoryScope || ms is DialogClassMemoryScope)))
                {
                    var memory = memoryScope.GetMemory(dc);
                    Assert.IsNotNull(memory, "should get memory without any set");
                    ObjectPath.SetPathValue(memory, "test", 15);
                    memory = memoryScope.GetMemory(dc);
                    Assert.AreEqual(15, ObjectPath.GetPathValue<int>(memory, "test"), "Should roundtrip memory");
                    ObjectPath.SetPathValue(memory, "test", 25);
                    memory = memoryScope.GetMemory(dc);
                    Assert.AreEqual(25, ObjectPath.GetPathValue<int>(memory, "test"), "Should roundtrip memory2");
                    memory = memoryScope.GetMemory(dc);
                    ObjectPath.SetPathValue(memory, "source", "destination");
                    ObjectPath.SetPathValue(memory, "{source}", 24);
                    Assert.AreEqual(24, ObjectPath.GetPathValue<int>(memory, "{source}"), "Roundtrip computed path");
                    ObjectPath.RemovePathValue(memory, "{source}");
                    Assert.AreEqual(false, ObjectPath.TryGetPathValue<int>(memory, "{source}", out var _), "Removed computed path");
                    ObjectPath.RemovePathValue(memory, "source");
                    Assert.AreEqual(false, ObjectPath.TryGetPathValue<int>(memory, "{source}", out var _), "No computed path");
                }
            }).StartTestAsync();
        }

        public class Foo
        {
            public Foo()
            {
            }

            public StringExpression Title { get; set; }
        }

        public class ComplexDialog : Dialog
        {
            public ComplexDialog()
            {
            }

            public StringExpression String { get; set; }

            public ObjectExpression<Foo> Foo { get; set; }

            public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                dc.Context.SendActivityAsync(dc.State.GetValue<string>("class.id"));
                dc.Context.SendActivityAsync(dc.State.GetValue<string>("dialogclass.id"));
                dc.Context.SendActivityAsync(dc.State.GetValue<string>("class.String"));
                dc.Context.SendActivityAsync(dc.State.GetValue<string>("class.foo.title"));
                return dc.EndDialogAsync();
            }
        }

        [TestMethod]
        public async Task ClassMemoryScopeTest()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(new MemoryStorage())
                .UseState(new UserState(new MemoryStorage()), new ConversationState(new MemoryStorage()));
            DialogManager dm = new DialogManager(new AdaptiveDialog("adaptiveDialog")
            {
                Triggers = new List<Adaptive.Conditions.OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new ComplexDialog()
                            {
                                Id = "test",
                                String = "='12345'",
                                Foo = new Foo() { Title = "='abcde'" }
                            }
                        }
                    }
                }
            })
            .UseResourceExplorer(new ResourceExplorer())
            .UseLanguageGeneration();

            await new TestFlow(adapter, (context, ct) =>
            {
                return dm.OnTurnAsync(context, ct);
            })
            .SendConversationUpdate()
                .AssertReply("test")
                .AssertReply("adaptiveDialog")
                .AssertReply("12345")
                .AssertReply("abcde")
            .StartTestAsync();
        }

        internal class BotStateTestDialog : Dialog
        {
            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
            {
                {
                    var botState = dc.Context.TurnState.Get<ConversationState>();
                    var property = botState.CreateProperty<string>("test");
                    await property.SetAsync(dc.Context, "cool").ConfigureAwait(false);

                    var result = dc.State.GetValue<string>("conversation.test");
                    Assert.AreEqual("cool", result);
                    dc.State.SetValue("conversation.test", "cool2");
                    Assert.AreEqual("cool2", await property.GetAsync(dc.Context));
                }

                {
                    var botState = dc.Context.TurnState.Get<UserState>();
                    var property = botState.CreateProperty<string>("test");
                    await property.SetAsync(dc.Context, "cool").ConfigureAwait(false);

                    var result = dc.State.GetValue<string>("user.test");
                    Assert.AreEqual("cool", result);
                    dc.State.SetValue("user.test", "cool2");
                    Assert.AreEqual("cool2", await property.GetAsync(dc.Context));
                }

                await dc.Context.SendActivityAsync("next");
                return await dc.EndDialogAsync();
            }
        }

        [TestMethod]
        public async Task BotStateScopes()
        {
            var storage = new MemoryStorage();
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .UseStorage(storage)
                .UseState(new UserState(storage), new ConversationState(storage))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            DialogManager dm = new DialogManager(new BotStateTestDialog());

            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
               {
                   await dm.OnTurnAsync(turnContext);
               })
                .Send("hello")
                    .AssertReply("next")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task DialogMemoryScopeTest()
        {
            var storage = new MemoryStorage();
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .UseStorage(storage)
                .UseState(new UserState(storage), new ConversationState(storage))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            DialogManager dm = new DialogManager(new MemoryScopeTestDialog());

            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext);
            })
            .Send("hello")
                .AssertReply("next")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task SettingsMemoryScopeTest()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("test", "yoyo") })
                .AddJsonFile(@"test.settings.json")
                .Build();

            HostContext.Current.Set<IConfiguration>(configuration);

            var storage = new MemoryStorage();
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .UseStorage(storage)
                .UseState(new UserState(storage), new ConversationState(storage))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            DialogManager dm = new DialogManager(new SettingsScopeTestDialog());

            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext);
            })
            .Send("settings.test")
                .AssertReply("yoyo")
            .Send("settings.string")
                .AssertReply("test")
            .Send("settings.int")
                .AssertReply("3")
            .Send("settings.array[0]")
                .AssertReply("zero")
            .Send("settings.array[1]")
                .AssertReply("one")
            .Send("settings.array[2]")
                .AssertReply("two")
            .Send("settings.array[3]")
                .AssertReply("three")
            .Send("settings.fakeArray.0")
                .AssertReply("zero")
            .Send("settings.fakeArray.1")
                .AssertReply("one")
            .Send("settings.fakeArray.2")
                .AssertReply("two")
            .Send("settings.fakeArray.3")
                .AssertReply("three")
            .Send("settings.fakeArray.zzz")
                .AssertReply("cat")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestPathResolvers()
        {
            var storage = new MemoryStorage();
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .UseStorage(storage)
                .UseState(new UserState(storage), new ConversationState(storage))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            DialogManager dm = new DialogManager(new PathResolverTestDialog())
                .UseResourceExplorer(new ResourceExplorer())
                .UseLanguageGeneration();

            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext);
            })
            .Send("hello")
                .AssertReply("next")
            .StartTestAsync();
        }
    }

    internal class MemoryScopeTestDialog : Dialog
    {
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dsm = new DialogStateManager(dc);
            foreach (var scope in dsm.Configuration.MemoryScopes.Where(ms => !(ms is DialogMemoryScope) && ms.IncludeInSnapshot == true).Select(ms => ms.Name))
            {
                var path = $"{scope}.test";
                Assert.IsNull(dc.State.GetValue<string>(path), $"{path} should be null");
                dc.State.SetValue(path, scope);
                Assert.IsNotNull(dc.State.GetValue<string>(path), $"{path} should not be null");
                Assert.AreEqual(scope, dc.State.GetValue<string>(path), $"{path} should be {scope}");
            }

            await dc.Context.SendActivityAsync("next");
            return await dc.EndDialogAsync();
        }
    }

    internal class SettingsScopeTestDialog : Dialog
    {
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await dc.Context.SendActivityAsync(dc.State.GetValue<string>(dc.Context.Activity.Text));
            return await dc.EndDialogAsync();
        }
    }

    internal class PathResolverTestDialog : Dialog
    {
        private string[] entities = new string[] { "test1", "test2" };

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateSetValue(dc, "#test", "turn.recognized.intents.test");
            ValidateSetValue(dc, "$test", "dialog.test");
            ValidateSetValue(dc, "@@test", "turn.recognized.entities.test", entities);
            Assert.AreEqual("test1", dc.State.GetValue<string>("@test"));
            Assert.AreEqual("test2", dc.State.GetValue<string[]>("@@test")[1]);

            ValidateRemoveValue(dc, "#test", "turn.recognized.intents.test");
            ValidateRemoveValue(dc, "$test", "dialog.test");
            ValidateValue(dc, "@test", "turn.recognized.entities.test.first()");
            ValidateRemoveValue(dc, "@@test", "turn.recognized.entities.test");

            await dc.Context.SendActivityAsync("next");
            return await dc.EndDialogAsync();
        }

        private void ValidateSetValue(DialogContext dc, string alias, string path, object value = null)
        {
            Assert.IsNull(dc.State.GetValue<object>(alias), $"{alias} should be null");
            dc.State.SetValue(path, value ?? alias);
            ValidateValue(dc, alias, path);
        }

        private void ValidateValue(DialogContext dc, string alias, string path)
        {
            var p = dc.State.GetValue<object>(path);
            Assert.IsNotNull(p);
            var a = dc.State.GetValue<object>(alias);
            Assert.IsNotNull(a);

            Assert.AreEqual(JsonConvert.SerializeObject(p), JsonConvert.SerializeObject(a), $"{alias} should be same as {path}");
        }

        private void ValidateRemoveValue(DialogContext dc, string alias, string path)
        {
            ValidateValue(dc, alias, path);
            dc.State.RemoveValue(alias);
            Assert.IsNull(dc.State.GetValue<object>(path), $"property should be removed by alias {alias}");
            Assert.IsNull(dc.State.GetValue<object>(alias), $"property should be removed by alias {alias}");
        }
    }
}
