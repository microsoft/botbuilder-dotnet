// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
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
                var dsm = dc.GetState() as DialogStateManager;
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
                Assert.IsNull(dc.GetState().GetValue<string>(path), $"{path} should be null");
                dc.GetState().SetValue(path, scope);
                Assert.IsNotNull(dc.GetState().GetValue<string>(path), $"{path} should not be null");
                Assert.AreEqual(scope, dc.GetState().GetValue<string>(path), $"{path} should be {scope}");
            }

            await dc.Context.SendActivityAsync("next");
            return await dc.EndDialogAsync();
        }
    }

    internal class SettingsScopeTestDialog : Dialog
    {
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await dc.Context.SendActivityAsync(dc.GetState().GetValue<string>(dc.Context.Activity.Text));
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
            Assert.AreEqual("test1", dc.GetState().GetValue<string>("@test"));
            Assert.AreEqual("test2", dc.GetState().GetValue<string[]>("@@test")[1]);

            ValidateRemoveValue(dc, "#test", "turn.recognized.intents.test");
            ValidateRemoveValue(dc, "$test", "dialog.test");
            ValidateValue(dc, "@test", "turn.recognized.entities.test.first()");
            ValidateRemoveValue(dc, "@@test", "turn.recognized.entities.test");

            await dc.Context.SendActivityAsync("next");
            return await dc.EndDialogAsync();
        }

        private void ValidateSetValue(DialogContext dc, string alias, string path, object value = null)
        {
            Assert.IsNull(dc.GetState().GetValue<object>(alias), $"{alias} should be null");
            dc.GetState().SetValue(path, value ?? alias);
            ValidateValue(dc, alias, path);
        }

        private void ValidateValue(DialogContext dc, string alias, string path)
        {
            var p = dc.GetState().GetValue<object>(path);
            Assert.IsNotNull(p);
            var a = dc.GetState().GetValue<object>(alias);
            Assert.IsNotNull(a);

            Assert.AreEqual(JsonConvert.SerializeObject(p), JsonConvert.SerializeObject(a), $"{alias} should be same as {path}");
        }

        private void ValidateRemoveValue(DialogContext dc, string alias, string path)
        {
            ValidateValue(dc, alias, path);
            dc.GetState().RemoveValue(alias);
            Assert.IsNull(dc.GetState().GetValue<object>(path), $"property should be removed by alias {alias}");
            Assert.IsNull(dc.GetState().GetValue<object>(alias), $"property should be removed by alias {alias}");
        }
    }
}
