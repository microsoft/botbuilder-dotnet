// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Bot.Builder.Testing.Tests.XUnit
{
    public class XUnitOutputMiddlewareTests
    {
        [Theory]
        [InlineData("Hi", "text reply 1", "speak reply 1", InputHints.AcceptingInput)]
        [InlineData("Hi", "text reply 2", "speak reply 2", InputHints.IgnoringInput)]
        [InlineData("Hi", "text reply 3", "speak reply 3", InputHints.ExpectingInput)]
        public async Task ShouldLogIncomingAndOutgoingMessageActivities(string utterance, string textReply, string speakReply, string inputHint)
        {
            var mockOutput = new MockTestOutputHelper();
            var sut = new XUnitDialogTestLogger(mockOutput);
            var testClient = new DialogTestClient(Channels.Test, new EchoDialog(textReply, speakReply, inputHint), null, new List<IMiddleware> { sut });
            await testClient.SendActivityAsync<IMessageActivity>(utterance);

            Assert.Equal("\r\nUser:  Hi", mockOutput.Output[0]);
            Assert.StartsWith("       -> ts: ", mockOutput.Output[1]);
            Assert.StartsWith($"\r\nBot:   Text = {textReply}\r\n       Speak = {speakReply}\r\n       InputHint = {inputHint}", mockOutput.Output[2]);
            Assert.StartsWith("       -> ts: ", mockOutput.Output[3]);
            Assert.Contains("elapsed", mockOutput.Output[3]);
        }

        [Theory]
        [InlineData(ActivityTypes.ContactRelationUpdate)]
        [InlineData(ActivityTypes.ConversationUpdate)]
        [InlineData(ActivityTypes.Typing)]
        [InlineData(ActivityTypes.EndOfConversation)]
        [InlineData(ActivityTypes.Event)]
        [InlineData(ActivityTypes.Invoke)]
        [InlineData(ActivityTypes.DeleteUserData)]
        [InlineData(ActivityTypes.MessageUpdate)]
        [InlineData(ActivityTypes.MessageDelete)]
        [InlineData(ActivityTypes.InstallationUpdate)]
        [InlineData(ActivityTypes.MessageReaction)]
        [InlineData(ActivityTypes.Suggestion)]
        [InlineData(ActivityTypes.Trace)]
        [InlineData(ActivityTypes.Handoff)]
        public async Task ShouldLogOtherIncomingAndOutgoingActivitiesAsRawJson(string activityType)
        {
            var mockOutput = new MockTestOutputHelper();
            var sut = new XUnitDialogTestLogger(mockOutput);
            var testClient = new DialogTestClient(Channels.Test, new EchoDialog(), null, new List<IMiddleware> { sut });

            var activity = new Activity(activityType);
            await testClient.SendActivityAsync<IActivity>(activity);

            Assert.Equal($"\r\nUser:  Activity = ActivityTypes.{activityType}", mockOutput.Output[0]);
            Assert.StartsWith(activityType, JsonConvert.DeserializeObject<Activity>(mockOutput.Output[1]).Type);
            Assert.StartsWith("       -> ts: ", mockOutput.Output[2]);
            Assert.Equal($"\r\nBot:   Activity = ActivityTypes.{activityType}", mockOutput.Output[3]);
            Assert.StartsWith(activityType, JsonConvert.DeserializeObject<Activity>(mockOutput.Output[4]).Type);
            Assert.StartsWith("       -> ts: ", mockOutput.Output[5]);
            Assert.Contains("elapsed", mockOutput.Output[5]);
        }

        private class EchoDialog : Dialog
        {
            private readonly string _textReply;
            private readonly string _speakReply;
            private readonly string _inputHint;

            public EchoDialog(string textReply = null, string speakReply = null, string inputHint = null)
                : base("testDialog")
            {
                _textReply = textReply;
                _speakReply = speakReply;
                _inputHint = inputHint;
            }

            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                var echoActivity = dc.Context.Activity;
                if (dc.Context.Activity.Type == ActivityTypes.Message)
                {
                    echoActivity = new Activity
                    {
                        Type = ActivityTypes.Message,
                        Text = _textReply,
                        Speak = _speakReply,
                        InputHint = _inputHint,
                    };
                }

                await dc.Context.SendActivityAsync(echoActivity, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Complete);
            }
        }

        private class MockTestOutputHelper : ITestOutputHelper
        {
            public MockTestOutputHelper()
            {
                Output = new List<string>();
            }

            public List<string> Output { get; }

            public void WriteLine(string message)
            {
                Output.Add(message);
            }

            public void WriteLine(string format, params object[] args)
            {
                Output.Add(string.Format(format, args));
            }
        }
    }
}
