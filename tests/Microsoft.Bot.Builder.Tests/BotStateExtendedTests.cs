using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Tests
{
    public class BotStateExtendedTests
    {
        public class TestBotState : BotStateExtended
        {
            public TestBotState(ITurnContextAwareStorage storage)
                : base(storage, $"BotStateExtended:{typeof(BotState).Namespace}.{typeof(BotState).Name}")
            {
            }

            [TestMethod]
            public async Task BotStateExtendedGetCachedState()
            {
                var turnContext = TestUtilities.CreateEmptyContext();
                turnContext.Activity.Conversation = new ConversationAccount { Id = "1234" };
                var dictionary = new Dictionary<string, object>();
                var storeCount = 0;
                var mockITurnContextAwareStorage = new Mock<ITurnContextAwareStorage>();
                mockITurnContextAwareStorage.Setup(ms => ms.ReadAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(result: (IDictionary<string, object>)dictionary));
                mockITurnContextAwareStorage.Setup(ms => ms.WriteAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask)
                    .Callback(() => storeCount++);

                var botState = new TestBotState(mockITurnContextAwareStorage.Object);

                (await botState
                    .CreateProperty<TestPocoState>("test-name")
                    .GetAsync(turnContext, () => new TestPocoState())).Value = "test-value";

                var cache = botState.GetCachedState(turnContext);

                Assert.IsNotNull(cache);

                Assert.AreSame(
                    cache,
                    botState.GetCachedState(turnContext),
                    "Subsequent call to GetCachedState returned a different instance");
            }

            [TestMethod]
            [Description("Should call IStorage.WriteAsync when force flag is true and cached state has not changed")]
            public async Task State_ForceCallsSaveWithoutCachedBotStateChanges()
            {
                // Mock a storage provider, which counts writes
                var storeCount = 0;
                var dictionary = new Dictionary<string, object>();
                var mock = new Mock<IStorage>();
                mock.Setup(ms => ms.WriteAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask)
                    .Callback(() => storeCount++);
                mock.Setup(ms => ms.ReadAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(result: (IDictionary<string, object>)dictionary));

                // Arrange
                var userState = new UserState(mock.Object);
                var context = TestUtilities.CreateEmptyContext();

                // Act
                var propertyA = userState.CreateProperty<string>("propertyA");

                // Set initial value and save
                await propertyA.SetAsync(context, "test");
                await userState.SaveChangesAsync(context);

                // Assert
                Assert.AreEqual(1, storeCount);

                // Saving without changes and wthout force does NOT call .WriteAsync
                await userState.SaveChangesAsync(context);
                Assert.AreEqual(1, storeCount);

                // Forcing save without changes DOES call .WriteAsync
                await userState.SaveChangesAsync(context, true);
                Assert.AreEqual(2, storeCount);
            }

            protected override string GetStorageKey(ITurnContext turnContext) => $"botstate/{turnContext.Activity.ChannelId}/{turnContext.Activity.Conversation.Id}/{typeof(BotState).Namespace}.{typeof(BotState).Name}";
        }
    }
}
