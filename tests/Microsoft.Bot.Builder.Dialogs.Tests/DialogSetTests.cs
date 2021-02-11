// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class DialogSetTests
    {
        [Fact]
        public void DialogSet_ConstructorValid()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            new DialogSet(dialogStateProperty);
        }

        [Fact]
        public void DialogSet_ConstructorNullProperty()
        {
            Assert.Throws<ArgumentNullException>(() => new DialogSet(null));
        }

        [Fact]
        public async Task DialogSet_CreateContextAsync()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var ds = new DialogSet(dialogStateProperty);
            var context = TestUtilities.CreateEmptyContext();
            await ds.CreateContextAsync(context);
        }

        [Fact]
        public async Task DialogSet_NullCreateContextAsync()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var ds = new DialogSet(dialogStateProperty);
            var context = TestUtilities.CreateEmptyContext();
            await ds.CreateContextAsync(context);
        }

        [Fact]
        public async Task DialogSet_AddWorks()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var ds = new DialogSet(dialogStateProperty)
                .Add(new WaterfallDialog("A"))
                .Add(new WaterfallDialog("B"));
            Assert.NotNull(ds.Find("A"));
            Assert.NotNull(ds.Find("B"));
            Assert.Null(ds.Find("C"));
            await Task.CompletedTask;
        }

        [Fact]
        public void DialogSet_GetVersion()
        {
            var ds = new DialogSet();
            var version1 = ds.GetVersion();
            Assert.NotNull(version1);

            var ds2 = new DialogSet();
            var version2 = ds.GetVersion();
            Assert.NotNull(version2);
            Assert.Equal(version1, version2);

            ds2.Add(new LamdaDialog((dc, ct) => null) { Id = "A" });
            var version3 = ds2.GetVersion();
            Assert.NotNull(version3);
            Assert.NotEqual(version2, version3);

            var version4 = ds2.GetVersion();
            Assert.NotNull(version3);
            Assert.Equal(version3, version4);

            var ds3 = new DialogSet()
                .Add(new LamdaDialog((dc, ct) => null) { Id = "A" });

            var version5 = ds3.GetVersion();
            Assert.NotNull(version5);
            Assert.Equal(version5, version4);
        }

        [Fact]
        public async Task DialogSet_TelemetrySet()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var ds = new DialogSet(dialogStateProperty)
                .Add(new WaterfallDialog("A"))
                .Add(new WaterfallDialog("B"));
            Assert.Equal(typeof(NullBotTelemetryClient), ds.Find("A").TelemetryClient.GetType());
            Assert.Equal(typeof(NullBotTelemetryClient), ds.Find("B").TelemetryClient.GetType());

            var botTelemetryClient = new MyBotTelemetryClient();
            ds.TelemetryClient = botTelemetryClient;

            Assert.Equal(typeof(MyBotTelemetryClient), ds.Find("A").TelemetryClient.GetType());
            Assert.Equal(typeof(MyBotTelemetryClient), ds.Find("B").TelemetryClient.GetType());
            await Task.CompletedTask;
        }

        [Fact]
        public async Task DialogSet_NullTelemetrySet()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var ds = new DialogSet(dialogStateProperty)
                .Add(new WaterfallDialog("A"))
                .Add(new WaterfallDialog("B"));

            ds.TelemetryClient = new MyBotTelemetryClient();
            ds.TelemetryClient = null;
            Assert.Equal(typeof(NullBotTelemetryClient), ds.Find("A").TelemetryClient.GetType());
            Assert.Equal(typeof(NullBotTelemetryClient), ds.Find("B").TelemetryClient.GetType());
            await Task.CompletedTask;
        }

        [Fact]
        public async Task DialogSet_AddTelemetrySet()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var ds = new DialogSet(dialogStateProperty)
                .Add(new WaterfallDialog("A"))
                .Add(new WaterfallDialog("B"));

            ds.TelemetryClient = new MyBotTelemetryClient();
            ds.Add(new WaterfallDialog("C"));

            Assert.Equal(typeof(MyBotTelemetryClient), ds.Find("C").TelemetryClient.GetType());
            await Task.CompletedTask;
        }

        [Fact]
        public async Task DialogSet_AddTelemetrySet_OnCyclicalDialogStructures()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");

            var component1 = new ComponentDialog("component1");
            var component2 = new ComponentDialog("component2");

            component1.Dialogs.Add(component2);
            component2.Dialogs.Add(component1);

            // Without the check in DialogSet setter, this test throws StackOverflowException.
            component1.Dialogs.TelemetryClient = new MyBotTelemetryClient();
            await Task.CompletedTask;
        }

        [Fact]
        public async Task DialogSet_HeterogeneousLoggers()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var ds = new DialogSet(dialogStateProperty)
                .Add(new WaterfallDialog("A"))
                .Add(new WaterfallDialog("B"));
            ds.Add(new WaterfallDialog("C"));

            // Make sure we can override (after Adding) the TelemetryClient and "sticks"
            ds.Find("C").TelemetryClient = new MyBotTelemetryClient();

            Assert.Equal(typeof(NullBotTelemetryClient), ds.Find("A").TelemetryClient.GetType());
            Assert.Equal(typeof(NullBotTelemetryClient), ds.Find("B").TelemetryClient.GetType());
            Assert.Equal(typeof(MyBotTelemetryClient), ds.Find("C").TelemetryClient.GetType());
            await Task.CompletedTask;
        }

        private class MyBotTelemetryClient : IBotTelemetryClient, IBotPageViewTelemetryClient
        {
            public MyBotTelemetryClient()
            {
            }

            public void Flush()
            {
                throw new NotImplementedException();
            }

            public void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string message = null, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
            {
                throw new NotImplementedException();
            }

            public void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
            {
                throw new NotImplementedException();
            }

            public void TrackPageView(string dialogName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
            {
                throw new NotImplementedException();
            }

            public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
            {
                throw new NotImplementedException();
            }

            public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
            {
                throw new NotImplementedException();
            }

            public void TrackTrace(string message, Severity severityLevel, IDictionary<string, string> properties)
            {
                throw new NotImplementedException();
            }
        }
    }
}
