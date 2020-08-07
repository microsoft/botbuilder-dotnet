// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class TurnContextTests
    {
        [Fact]
        public void ConstructorNullAdapter()
        {
            Assert.Throws<ArgumentNullException>(() => new TurnContext(null, new Activity()));
        }

        [Fact]
        public void ConstructorNullActivity()
        {
            var a = new TestAdapter(TestAdapter.CreateConversation("ConstructorNullActivity"));
            Assert.Throws<ArgumentNullException>(() => new TurnContext(a, null));
        }

        [Fact]
        public void Constructor()
        {
            var c = new TurnContext(new TestAdapter(TestAdapter.CreateConversation("Constructor")), new Activity());
            Assert.NotNull(c);
        }

        [Fact]
        public void RespondedIsFalse()
        {
            var c = new TurnContext(new TestAdapter(TestAdapter.CreateConversation("RespondedIsFalse")), new Activity());
            Assert.False(c.Responded);
        }

        [Fact]
        public async Task CacheValueUsingSetAndGet()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("CacheValueUsingSetAndGet"));
            await new TestFlow(adapter, MyBotLogic)
                    .Send("TestResponded")
                    .StartTestAsync();
        }

        [Fact]
        public void GetThrowsOnNullKey()
        {
            var c = new TurnContext(new SimpleAdapter(), new Activity());
            Assert.Throws<ArgumentNullException>(() => c.TurnState.Get<object>(null));
        }

        [Fact]
        public void GetReturnsNullOnEmptyKey()
        {
            var c = new TurnContext(new SimpleAdapter(), new Activity());
            var service = c.TurnState.Get<object>(string.Empty); // empty key
            Assert.Null(service);
        }

        [Fact]
        public void GetReturnsNullWithUnknownKey()
        {
            var c = new TurnContext(new SimpleAdapter(), new Activity());
            var o = c.TurnState.Get<object>("test");
            Assert.Null(o);
        }

        [Fact]
        public void TestTurnContextLocale()
        {
            var c = new TurnContext(new SimpleAdapter(), new Activity());

            c.Locale = "fr-FR";

            Assert.Equal("fr-FR", c.Locale);
        }

        [Fact]
        public void CacheValueUsingGetAndSet()
        {
            var c = new TurnContext(new SimpleAdapter(), new Activity());

            c.TurnState.Add("bar", "foo");
            var result = c.TurnState.Get<string>("bar");

            Assert.Equal("foo", result);
        }

        [Fact]
        public void CacheValueUsingGetAndSetGenericWithTypeAsKeyName()
        {
            var c = new TurnContext(new SimpleAdapter(), new Activity());

            c.TurnState.Add<string>("foo");
            string result = c.TurnState.Get<string>();

            Assert.Equal("foo", result);
        }

        [Fact]
        public void RequestIsSet()
        {
            var c = new TurnContext(new SimpleAdapter(), TestMessage.Message());
            Assert.True(c.Activity.Id == "1234");
        }

        [Fact]
        public async Task SendAndSetResponded()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());
            Assert.False(c.Responded);
            var response = await c.SendActivityAsync(TestMessage.Message("testtest"));

            Assert.True(c.Responded);
            Assert.True(response.Id == "testtest");
        }

        [Fact]
        public async Task SendBatchOfActivities()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());
            Assert.False(c.Responded);

            var message1 = TestMessage.Message("message1");
            var message2 = TestMessage.Message("message2");

            var response = await c.SendActivitiesAsync(new IActivity[] { message1, message2 });

            Assert.True(c.Responded);
            Assert.True(response.Length == 2);
            Assert.True(response[0].Id == "message1");
            Assert.True(response[1].Id == "message2");
        }

        [Fact]
        public async Task SendAndSetRespondedUsingIMessageActivity()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());
            Assert.False(c.Responded);

            var msg = TestMessage.Message().AsMessageActivity();
            await c.SendActivityAsync(msg);
            Assert.True(c.Responded);
        }

        [Fact]
        public async Task TraceActivitiesDoNoSetResponded()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());
            Assert.False(c.Responded);

            // Send a Trace Activity, and make sure responded is NOT set. 
            var trace = Activity.CreateTraceActivity("trace");
            await c.SendActivityAsync(trace);
            Assert.False(c.Responded);

            // Just to sanity check everything, send a Message and verify the
            // responded flag IS set.
            var msg = TestMessage.Message().AsMessageActivity();
            await c.SendActivityAsync(msg);
            Assert.True(c.Responded);
        }

        [Fact]
        public async Task SendOneActivityToAdapter()
        {
            bool foundActivity = false;

            void ValidateResponses(Activity[] activities)
            {
                Assert.True(activities.Count() == 1, "Incorrect Count");
                Assert.True(activities[0].Id == "1234");
                foundActivity = true;
            }

            var a = new SimpleAdapter(ValidateResponses);
            var c = new TurnContext(a, new Activity());
            await c.SendActivityAsync(TestMessage.Message());
            Assert.True(foundActivity);
        }

        [Fact]
        public async Task CallOnSendBeforeDelivery()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());

            int count = 0;
            c.OnSendActivities(async (context, activities, next) =>
            {
                Assert.NotNull(activities); // Null Array passed in
                count = activities.Count();
                return await next();
            });

            await c.SendActivityAsync(TestMessage.Message());

            Assert.True(count == 1);
        }

        [Fact]
        public async Task AllowInterceptionOfDeliveryOnSend()
        {
            bool responsesSent = false;
            void ValidateResponses(Activity[] activities)
            {
                responsesSent = true;
                Assert.True(false); // Should not be called. Interceptor did not work
            }

            var a = new SimpleAdapter(ValidateResponses);
            var c = new TurnContext(a, new Activity());

            int count = 0;
            c.OnSendActivities((context, activities, next) =>
            {
                Assert.NotNull(activities); // Null Array passed in
                count = activities.Count();

                // Do not call next.
                return Task.FromResult<ResourceResponse[]>(null);
            });

            await c.SendActivityAsync(TestMessage.Message());

            Assert.True(count == 1);
            Assert.False(responsesSent, "Responses made it to the adapter.");
        }

        [Fact]
        public async Task InterceptAndMutateOnSend()
        {
            bool foundIt = false;
            void ValidateResponses(Activity[] activities)
            {
                Assert.NotNull(activities);
                Assert.True(activities.Length == 1);
                Assert.True(activities[0].Id == "changed");
                foundIt = true;
            }

            var a = new SimpleAdapter(ValidateResponses);
            var c = new TurnContext(a, new Activity());

            c.OnSendActivities(async (context, activities, next) =>
            {
                Assert.NotNull(activities); // Null Array passed in
                Assert.True(activities.Count() == 1);
                Assert.True(activities[0].Id == "1234", "Unknown Id Passed In");
                activities[0].Id = "changed";
                return await next();
            });

            await c.SendActivityAsync(TestMessage.Message());

            // Intercepted the message, changed it, and sent it on to the Adapter
            Assert.True(foundIt);
        }

        [Fact]
        public async Task UpdateOneActivityToAdapter()
        {
            bool foundActivity = false;

            void ValidateUpdate(Activity activity)
            {
                Assert.NotNull(activity);
                Assert.True(activity.Id == "test");
                foundActivity = true;
            }

            var a = new SimpleAdapter(ValidateUpdate);
            var c = new TurnContext(a, new Activity());

            var message = TestMessage.Message("test");
            var updateResult = await c.UpdateActivityAsync(message);

            Assert.True(foundActivity);
            Assert.True(updateResult.Id == "test");
        }

        [Fact]
        public async Task UpdateActivityWithMessageFactory()
        {
            const string ACTIVITY_ID = "activity ID";
            const string CONVERSATION_ID = "conversation ID";

            var foundActivity = false;

            void ValidateUpdate(Activity activity)
            {
                Assert.NotNull(activity);
                Assert.True(activity.Id == ACTIVITY_ID);
                Assert.True(activity.Conversation.Id == CONVERSATION_ID);
                foundActivity = true;
            }

            var a = new SimpleAdapter(ValidateUpdate);
            var c = new TurnContext(a, new Activity(conversation: new ConversationAccount(id: CONVERSATION_ID)));

            var message = MessageFactory.Text("test text");

            message.Id = ACTIVITY_ID;

            var updateResult = await c.UpdateActivityAsync(message);

            Assert.True(foundActivity);
            Assert.True(updateResult.Id == ACTIVITY_ID);

            c.Dispose();
        }

        [Fact]
        public async Task CallOnUpdateBeforeDelivery()
        {
            bool foundActivity = false;

            void ValidateUpdate(Activity activity)
            {
                Assert.NotNull(activity);
                Assert.True(activity.Id == "1234");
                foundActivity = true;
            }

            SimpleAdapter a = new SimpleAdapter(ValidateUpdate);
            TurnContext c = new TurnContext(a, new Activity());

            bool wasCalled = false;
            c.OnUpdateActivity(async (context, activity, next) =>
            {
                Assert.NotNull(activity); // Null activity passed in
                wasCalled = true;
                return await next();
            });
            await c.UpdateActivityAsync(TestMessage.Message());
            Assert.True(wasCalled);
            Assert.True(foundActivity);
        }

        [Fact]
        public async Task InterceptOnUpdate()
        {
            bool adapterCalled = false;
            void ValidateUpdate(Activity activity)
            {
                adapterCalled = true;
                Assert.True(false); // Should not be called
            }

            var a = new SimpleAdapter(ValidateUpdate);
            var c = new TurnContext(a, new Activity());

            bool wasCalled = false;
            c.OnUpdateActivity((context, activity, next) =>
            {
                Assert.NotNull(activity); // Null activity passed in
                wasCalled = true;

                // Do Not Call Next
                return Task.FromResult<ResourceResponse>(null);
            });

            await c.UpdateActivityAsync(TestMessage.Message());
            Assert.True(wasCalled); // Interceptor was called
            Assert.False(adapterCalled); // Adapter was not
        }

        [Fact]
        public async Task InterceptAndMutateOnUpdate()
        {
            bool adapterCalled = false;
            void ValidateUpdate(Activity activity)
            {
                Assert.True(activity.Id == "mutated");
                adapterCalled = true;
            }

            var a = new SimpleAdapter(ValidateUpdate);
            var c = new TurnContext(a, new Activity());

            c.OnUpdateActivity(async (context, activity, next) =>
            {
                Assert.NotNull(activity); // Null activity passed in
                Assert.True(activity.Id == "1234");
                activity.Id = "mutated";
                return await next();
            });

            await c.UpdateActivityAsync(TestMessage.Message());
            Assert.True(adapterCalled); // Adapter was not
        }

        [Fact]
        public async Task DeleteOneActivityToAdapter()
        {
            bool deleteCalled = false;

            void ValidateDelete(ConversationReference r)
            {
                Assert.NotNull(r);
                Assert.True(r.ActivityId == "12345");
                deleteCalled = true;
            }

            var a = new SimpleAdapter(ValidateDelete);
            var c = new TurnContext(a, TestMessage.Message());
            await c.DeleteActivityAsync("12345");
            Assert.True(deleteCalled);
        }

        [Fact]
        public async Task DeleteConversationReferenceToAdapter()
        {
            bool deleteCalled = false;

            void ValidateDelete(ConversationReference r)
            {
                Assert.NotNull(r);
                Assert.True(r.ActivityId == "12345");
                deleteCalled = true;
            }

            var a = new SimpleAdapter(ValidateDelete);
            var c = new TurnContext(a, TestMessage.Message());

            var reference = new ConversationReference("12345");

            await c.DeleteActivityAsync(reference);
            Assert.True(deleteCalled);
        }

        [Fact]
        public async Task InterceptOnDelete()
        {
            bool adapterCalled = false;

            void ValidateDelete(ConversationReference r)
            {
                adapterCalled = true;
                Assert.True(false); // Should not be called
            }

            var a = new SimpleAdapter(ValidateDelete);
            var c = new TurnContext(a, new Activity());

            bool wasCalled = false;
            c.OnDeleteActivity((context, convRef, next) =>
            {
                Assert.NotNull(convRef); // Null activity passed in
                wasCalled = true;

                // Do Not Call Next
                return Task.FromResult<ResourceResponse[]>(null);
            });

            await c.DeleteActivityAsync("1234");
            Assert.True(wasCalled); // Interceptor was called
            Assert.False(adapterCalled); // Adapter was not
        }

        [Fact]
        public async Task InterceptAndMutateOnDelete()
        {
            bool adapterCalled = false;

            void ValidateDelete(ConversationReference r)
            {
                Assert.True(r.ActivityId == "mutated");
                adapterCalled = true;
            }

            var a = new SimpleAdapter(ValidateDelete);
            var c = new TurnContext(a, new Activity());

            c.OnDeleteActivity(async (context, convRef, next) =>
            {
                Assert.NotNull(convRef); // Null activity passed in
                Assert.True(convRef.ActivityId == "1234", "Incorrect Activity Id");
                convRef.ActivityId = "mutated";
                await next();
            });

            await c.DeleteActivityAsync("1234");
            Assert.True(adapterCalled); // Adapter was called + valided the change
        }

        [Fact]
        public async Task ThrowExceptionInOnSend()
        {
            var a = new SimpleAdapter();
            var c = new TurnContext(a, new Activity());

            c.OnSendActivities((context, activities, next) =>
            {
                throw new Exception("test");
            });

            try
            {
                await c.SendActivityAsync(TestMessage.Message());
                Assert.True(false); // Should not get here
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == "test");
            }
        }

        public async Task MyBotLogic(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.AsMessageActivity().Text)
            {
                case "count":
                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("one"));
                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("two"));
                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("three"));
                    break;
                case "ignore":
                    break;
                case "TestResponded":
                    if (turnContext.Responded == true)
                    {
                        throw new InvalidOperationException("Responded Is True");
                    }

                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("one"));

                    if (turnContext.Responded == false)
                    {
                        throw new InvalidOperationException("Responded Is True");
                    }

                    break;
                default:
                    await turnContext.SendActivityAsync(
                        turnContext.Activity.CreateReply($"echo:{turnContext.Activity.Text}"));
                    break;
            }
        }
    }
}
