// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("State Management")]
    public class BotStateSetTests
    {
        [TestMethod]
        public void BotStateSet_Properties()
        {
            var storage = new MemoryStorage();

            // setup userstate
            var userState = new UserState(storage);
            var userProperty = userState.CreateProperty<int>("userCount");

            // setup convState
            var convState = new ConversationState(storage);
            var convProperty = convState.CreateProperty<int>("convCount");

            var stateSet = new BotStateSet(userState, convState);

            Assert.AreEqual(stateSet.BotStates.Count, 2);
            Assert.IsNotNull(stateSet.BotStates.OfType<UserState>().First());
            Assert.IsNotNull(stateSet.BotStates.OfType<ConversationState>().First());
        }

        [TestMethod]
        public async Task BotStateSet_LoadAsync()
        {
            var storage = new MemoryStorage();

            var turnContext = TestUtilities.CreateEmptyContext();
            {
                // setup userstate
                var userState = new UserState(storage);
                var userProperty = userState.CreateProperty<int>("userCount");

                // setup convState
                var convState = new ConversationState(storage);
                var convProperty = convState.CreateProperty<int>("convCount");

                var stateSet = new BotStateSet(userState, convState);

                Assert.AreEqual(stateSet.BotStates.Count, 2);

                var userCount = await userProperty.GetAsync(turnContext, () => 0);
                Assert.AreEqual(0, userCount);
                var convCount = await convProperty.GetAsync(turnContext, () => 0);
                Assert.AreEqual(0, convCount);

                await userProperty.SetAsync(turnContext, 10);
                await convProperty.SetAsync(turnContext, 20);

                await stateSet.SaveAllChangesAsync(turnContext);
            }

            {
                // setup userstate
                var userState = new UserState(storage);
                var userProperty = userState.CreateProperty<int>("userCount");

                // setup convState
                var convState = new ConversationState(storage);
                var convProperty = convState.CreateProperty<int>("convCount");

                var stateSet = new BotStateSet(userState, convState);

                await stateSet.LoadAllAsync(turnContext);

                var userCount = await userProperty.GetAsync(turnContext, () => 0);
                Assert.AreEqual(10, userCount);
                var convCount = await convProperty.GetAsync(turnContext, () => 0);
                Assert.AreEqual(20, convCount);
            }
        }

        [TestMethod]
        public async Task BotStateSet_SaveAsync()
        {
            var storage = new MemoryStorage();

            // setup userstate
            var userState = new UserState(storage);
            var userProperty = userState.CreateProperty<int>("userCount");

            // setup convState
            var convState = new ConversationState(storage);
            var convProperty = convState.CreateProperty<int>("convCount");

            var stateSet = new BotStateSet(userState, convState);

            Assert.AreEqual(stateSet.BotStates.Count, 2);
            var context = TestUtilities.CreateEmptyContext();
            await stateSet.LoadAllAsync(context);

            var userCount = await userProperty.GetAsync(context, () => 0);
            Assert.AreEqual(0, userCount);
            var convCount = await convProperty.GetAsync(context, () => 0);
            Assert.AreEqual(0, convCount);

            await userProperty.SetAsync(context, 10);
            await convProperty.SetAsync(context, 20);

            await stateSet.SaveAllChangesAsync(context);

            userCount = await userProperty.GetAsync(context, () => 0);
            Assert.AreEqual(10, userCount);

            convCount = await convProperty.GetAsync(context, () => 0);
            Assert.AreEqual(20, convCount);
        }
    }
}
