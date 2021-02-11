using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TestBot.Shared.Bots;
using Microsoft.BotBuilderSamples.Tests.Framework;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Bots
{
    public class DialogBotTests
    {
        //[Fact]
        //public async Task LogsInformationToILogger()
        //{
        //    // Arrange
        //    var memoryStorage = new MemoryStorage();
        //    var conversationState = new ConversationState(memoryStorage);
        //    var userState = new UserState(memoryStorage);

        //    var mockRootDialog = SimpleMockFactory.CreateMockDialog<Dialog>(null, "mockRootDialog");
        //    var mockLogger = new Mock<ILogger<DialogBot<Dialog>>>();
        //    mockLogger.Setup(x =>
        //        x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));

        //    // Run the bot
        //    var sut = new DialogBot<Dialog>(conversationState, userState, mockRootDialog.Object, mockLogger.Object);
        //    var testAdapter = new TestAdapter();
        //    var testFlow = new TestFlow(testAdapter, sut);
        //    await testFlow.Send("Hi").StartTestAsync();

        //    // Assert that log was changed with the expected parameters
        //    mockLogger.Verify(
        //        x => x.Log(
        //            LogLevel.Information,
        //            It.IsAny<EventId>(),
        //            It.Is<object>(o => o.ToString() == "Running dialog with Message Activity."),
        //            null,
        //            It.IsAny<Func<object, Exception, string>>()),
        //        Times.Once);
        //}

        [Fact]
        public async Task SavesTurnStateUsingMockWithVirtualSaveChangesAsync()
        {
            // Note: this test requires that SaveChangesAsync is made virtual in order to be able to create a mock.
            var memoryStorage = new MemoryStorage();
            var mockConversationState = new Mock<ConversationState>(memoryStorage)
            {
                CallBase = true,
            };

            var mockUserState = new Mock<UserState>(memoryStorage)
            {
                CallBase = true,
            };

            var mockRootDialog = SimpleMockFactory.CreateMockDialog<Dialog>(null, "mockRootDialog");
            var mockLogger = new Mock<ILogger<DialogBot<Dialog>>>();

            // Act
            var sut = new DialogBot<Dialog>(mockConversationState.Object, mockUserState.Object, mockRootDialog.Object, mockLogger.Object);
            var testAdapter = new TestAdapter();
            var testFlow = new TestFlow(testAdapter, sut);
            await testFlow.Send("Hi").StartTestAsync();

            // Assert that SaveChangesAsync was called
            mockConversationState.Verify(x => x.SaveChangesAsync(It.IsAny<TurnContext>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            mockUserState.Verify(x => x.SaveChangesAsync(It.IsAny<TurnContext>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(Skip = "TODO: need to figure out how to implement this version of the test")]
        public async Task SavesTurnStateUsingMemoryStorage()
        {
            // TODO: Figure out how to implement this test.
            // Note: this doesn't require a virtual SaveChangesAsync and it manually inspects storage to ensure the save methods were called.
            var memoryStorage = new MemoryStorage();
            var conversationState = new ConversationState(memoryStorage);
            var userState = new UserState(memoryStorage);

            var mockRootDialog = new Mock<Dialog>("mockRootDialog");
            mockRootDialog.Setup(x => x.ContinueDialogAsync(It.IsAny<DialogContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new DialogTurnResult(DialogTurnStatus.Empty)));

            var mockLogger = new Mock<ILogger<DialogBot<Dialog>>>();

            // Run the bot
            var sut = new DialogBot<Dialog>(conversationState, userState, mockRootDialog.Object, mockLogger.Object);
            var testAdapter = new TestAdapter();
            var testFlow = new TestFlow(testAdapter, sut);
            await testFlow.Send("Hi").StartTestAsync();

            // Assert that SaveChangesAsyncWasCalled
            Assert.True(false, "TODO");
        }
    }
}
