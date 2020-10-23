// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TestBot.Shared;
using Microsoft.Bot.Builder.TestBot.Shared.Dialogs;
using Microsoft.Bot.Builder.TestBot.Shared.Services;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Tests.Framework;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    /// <summary>
    /// This sample uses the current classes and approach for testing bot conversations.
    /// Note: this is included just as a reference.
    /// </summary>
    public class MainDialogTestFlowTests : BotTestBase
    {
        [Fact(Skip = "Ignoring this one, this is just a sample on the old way of writing tests")]
        public async Task WholeEnchilada()
        {
            var mockFlightBookingService = new Mock<IFlightBookingService>();
            mockFlightBookingService.Setup(x => x.BookFlight(It.IsAny<BookingDetails>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            var mockBookingDialog = SimpleMockFactory.CreateMockDialog<BookingDialog>(null, mockFlightBookingService.Object).Object;
            var mockLogger = new Mock<ILogger<MainDialog>>();
            var sut = new MainDialog(mockLogger.Object, null, mockBookingDialog);

            var testFlow = BuildTestFlow(sut);

            await testFlow.Send("hi")
                .AssertReply("What can I help you with today?")
                .Send("hi")
                .AssertReply("Where would you like to travel to?")
                .Send("Bahamas")
                .AssertReply("Where are you traveling from?")
                .Send("New York")
                .AssertReply("When would you like to travel?")
                .Send("tomorrow at 5 PM")
                .AssertReply(activity =>
                {
                    // TODO: I had to add the Yes No for the channelId = test, the emulator displays suggested actions instead.
                    var message = (IMessageActivity)activity;
                    Assert.Equal(
                        "Please confirm, I have you traveling to: Bahamas from: New York on: 2019-04-18T17 (1) Yes or (2) No",
                        message.Text);
                })
                .Send("Yes")
                .AssertReply("I have you booked to Bahamas from New York on tomorrow 5PM")
                .StartTestAsync();
        }

        private static TestFlow BuildTestFlow(Dialog targetDialog)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var testAdapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));
            var dialogState = convoState.CreateProperty<DialogState>("DialogState");
            var testFlow = new TestFlow(testAdapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState(), cancellationToken);
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(targetDialog);

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                switch (results.Status)
                {
                    case DialogTurnStatus.Empty:
                        await dc.BeginDialogAsync(targetDialog.Id, null, cancellationToken);
                        break;
                    case DialogTurnStatus.Complete:
                    {
                        // TODO: Dialog has ended, figure out a way of asserting that this is the case.
                        break;
                    }
                }
            });
            return testFlow;
        }
    }
}
