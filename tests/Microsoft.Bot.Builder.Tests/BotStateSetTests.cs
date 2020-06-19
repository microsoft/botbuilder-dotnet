// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class BotStateSetTests
    {
        [Fact]
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

            Assert.Equal(2, stateSet.BotStates.Count);
            Assert.NotNull(stateSet.BotStates.OfType<UserState>().First());
            Assert.NotNull(stateSet.BotStates.OfType<ConversationState>().First());
        }

        [Fact]
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

                Assert.Equal(2, stateSet.BotStates.Count);

                var userCount = await userProperty.GetAsync(turnContext, () => 0);
                Assert.Equal(0, userCount);
                var convCount = await convProperty.GetAsync(turnContext, () => 0);
                Assert.Equal(0, convCount);

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
                Assert.Equal(10, userCount);
                var convCount = await convProperty.GetAsync(turnContext, () => 0);
                Assert.Equal(20, convCount);
            }
        }

        [Fact]
        public async Task BotStateSet_ReturnsDefaultForNullValueType()
        {
            var storage = new MemoryStorage();

            var turnContext = TestUtilities.CreateEmptyContext();

            // setup userstate
            var userState = new UserState(storage);
            var userProperty = userState.CreateProperty<SomeComplexType>("userStateObject");

            // setup convState
            var convState = new ConversationState(storage);
            var convProperty = convState.CreateProperty<SomeComplexType>("convStateObject");

            var stateSet = new BotStateSet(userState, convState);

            Assert.Equal(2, stateSet.BotStates.Count);

            var userObject = await userProperty.GetAsync(turnContext, () => null);
            Assert.Null(userObject);

            // Ensure we also get null on second attempt
            userObject = await userProperty.GetAsync(turnContext, () => null);
            Assert.Null(userObject);

            var convObject = await convProperty.GetAsync(turnContext, () => null);
            Assert.Null(convObject);

            // Ensure we also get null on second attempt
            convObject = await convProperty.GetAsync(turnContext, () => null);
            Assert.Null(convObject);
        }

        [Fact]
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

            Assert.Equal(2, stateSet.BotStates.Count);
            var context = TestUtilities.CreateEmptyContext();
            await stateSet.LoadAllAsync(context);

            var userCount = await userProperty.GetAsync(context, () => 0);
            Assert.Equal(0, userCount);
            var convCount = await convProperty.GetAsync(context, () => 0);
            Assert.Equal(0, convCount);

            await userProperty.SetAsync(context, 10);
            await convProperty.SetAsync(context, 20);

            await stateSet.SaveAllChangesAsync(context);

            userCount = await userProperty.GetAsync(context, () => 0);
            Assert.Equal(10, userCount);

            convCount = await convProperty.GetAsync(context, () => 0);
            Assert.Equal(20, convCount);
        }

        internal class SomeComplexType
        {
            public string PropA { get; set; }

            public int PropB { get; set; }
        }
    }
}
