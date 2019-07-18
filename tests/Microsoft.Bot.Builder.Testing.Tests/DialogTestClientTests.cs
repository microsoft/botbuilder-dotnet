// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Testing.Tests
{
    public class DialogTestClientTests
    {
        private readonly Mock<Dialog> _mockDialog;

        public DialogTestClientTests()
        {
            _mockDialog = new Mock<Dialog>("testDialog");
        }

        [Fact]
        public async Task ShouldInvokeContinueAndBegin()
        {
            _mockDialog
                .Setup(x => x.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new DialogTurnResult(DialogTurnStatus.Waiting)));
            _mockDialog
                .Setup(x => x.ContinueDialogAsync(It.IsAny<DialogContext>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new DialogTurnResult(DialogTurnStatus.Complete)));
            var sut = new DialogTestClient(Channels.Test, _mockDialog.Object);

            // Assert proper methods in the mock dialog have been called.
            await sut.SendActivityAsync<IMessageActivity>("test");
            _mockDialog.Verify(
                x => x.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Once());
            _mockDialog.Verify(
                x => x.ContinueDialogAsync(It.IsAny<DialogContext>(), It.IsAny<CancellationToken>()),
                Times.Never);

            // Assert proper methods in the mock dialog have been called on the next turn too.
            await sut.SendActivityAsync<IMessageActivity>("test 2");
            _mockDialog.Verify(
                x => x.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Once());
            _mockDialog.Verify(
                x => x.ContinueDialogAsync(It.IsAny<DialogContext>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ShouldSendActivityToDialogAndReceiveReply()
        {
            IActivity receivedActivity = null;
            const string testUtterance = "test";
            const string testReply = "I got your message";
            _mockDialog
                .Setup(x => x.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(async (DialogContext dc, object options, CancellationToken cancellationToken) =>
                {
                    receivedActivity = dc.Context.Activity;
                    await dc.Context.SendActivityAsync(testReply, cancellationToken: cancellationToken);
                    return new DialogTurnResult(DialogTurnStatus.Complete);
                });
            var sut = new DialogTestClient(Channels.Test, _mockDialog.Object);

            var reply = await sut.SendActivityAsync<IMessageActivity>(testUtterance);
            Assert.Equal(testUtterance, receivedActivity.AsMessageActivity().Text);
            Assert.Equal(testReply, reply.Text);
        }

        [Fact]
        public async Task ShouldSendInitialParameters()
        {
            var optionsSent = new TestOptions
            {
                SomeText = "Text",
                SomeNumber = 42,
            };
            object optionsReceived = null;
            _mockDialog
                .Setup(x => x.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns((DialogContext dc, object options, CancellationToken cancellationToken) =>
                {
                    optionsReceived = options;
                    return Task.FromResult(new DialogTurnResult(DialogTurnStatus.Complete));
                });
            var sut = new DialogTestClient(Channels.Test, _mockDialog.Object, optionsSent);

            await sut.SendActivityAsync<IMessageActivity>("test");
            Assert.NotNull(optionsReceived);
            Assert.Equal(optionsSent.SomeText, ((TestOptions)optionsReceived).SomeText);
            Assert.Equal(optionsSent.SomeNumber, ((TestOptions)optionsReceived).SomeNumber);
        }

        [Theory]
        [InlineData(DialogTurnStatus.Empty, "empty result")]
        [InlineData(DialogTurnStatus.Cancelled, "cancelled result")]
        [InlineData(DialogTurnStatus.Complete, "completed result")]
        [InlineData(DialogTurnStatus.Waiting, "waiting result")]
        public async Task ShouldExposeDialogTurnResults(DialogTurnStatus turnStatus, object turnResult)
        {
            _mockDialog
                .Setup(x => x.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new DialogTurnResult(turnStatus, turnResult)));
            var sut = new DialogTestClient(Channels.Test, _mockDialog.Object);

            await sut.SendActivityAsync<IMessageActivity>("test");
            Assert.Equal(turnStatus, sut.DialogTurnResult.Status);
            Assert.Equal(turnResult, sut.DialogTurnResult.Result);
        }

        [Fact]
        public async Task ShouldUseCustomAdapter()
        {
            var customAdapter = new Mock<TestAdapter>(Channels.Directline, false)
            {
                CallBase = true,
            };

            var sut = new DialogTestClient(customAdapter.Object, _mockDialog.Object);

            await sut.SendActivityAsync<IActivity>("test message");

            customAdapter.Verify(
                x => x.SendTextToBotAsync(
                    It.Is<string>(s => s == "test message"),
                    It.IsAny<BotCallbackHandler>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        private class TestOptions
        {
            public string SomeText { get; set; }

            public int SomeNumber { get; set; }
        }
    }
}
