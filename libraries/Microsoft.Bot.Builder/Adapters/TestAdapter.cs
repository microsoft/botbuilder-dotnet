// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters
{
    public class TestAdapter : ActivityAdapterBase
    {
        private int _nextId = 0;
        private readonly Queue<IActivity> botReplies = new Queue<IActivity>();

        public TestAdapter(ConversationReference reference = null)
        {
            if (reference != null)
            {
                this.ConversationReference = reference;
            }
            else
            {
                this.ConversationReference = new ConversationReference
                {
                    ChannelId = "test",
                    ServiceUrl = "https://test.com"
                };

                this.ConversationReference.User = new ChannelAccount("user1", "User1");
                this.ConversationReference.Bot = new ChannelAccount("bot", "Bot");
                this.ConversationReference.Conversation = new ConversationAccount(false, "convo1", "Conversation1");
            }
        }

        public ConversationReference ConversationReference { get; set; }


        /// <summary>
        /// get next activity or null if none
        /// </summary>
        /// <returns></returns>
        public IActivity GetNextReply()
        {
            lock (this.botReplies)
            {
                if (this.botReplies.Count > 0)
                    return this.botReplies.Dequeue();
            }
            return null;
        }

        public IActivity MakeActivity(string text = null)
        {
            Activity activity = new Activity
            {
                Type = ActivityTypes.Message,
                From = ConversationReference.User,
                Recipient = ConversationReference.Bot,
                Conversation = ConversationReference.Conversation,
                ServiceUrl = ConversationReference.ServiceUrl,
                Id = (_nextId++).ToString(),
                Text = text
            };

            return activity;
        }

        /// <summary>
        /// Bot posting an activity back to the source
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task Send(IList<IActivity> activities)
        {
            foreach (var activity in activities)
            {
                if (activity.Type == ActivityTypesEx.Delay)
                {
                    // The BotFrameworkAdapter and Console adapter implement this
                    // hack directly in the POST method. Replicating that here
                    // to keep the behavior as close as possible to facillitate
                    // more realistic tests.                     
                    int delayMs = (int)((Activity)activity).Value;
                    await Task.Delay(delayMs);
                }
                else
                {
                    lock (this.botReplies)
                    {
                        this.botReplies.Enqueue(activity);
                    }
                }
            }
        }

        /* INTERNAL */
        internal Task SendActivityToBot(string userSays)
        {
            return this.SendActivityToBot(this.MakeActivity(userSays));
        }

        internal Task SendActivityToBot(IActivity activity)
        {
            lock (this.ConversationReference)
            {
                // ready for next reply
                if (activity.Type == null)
                    activity.Type = ActivityTypes.Message;
                activity.ChannelId = this.ConversationReference.ChannelId;
                activity.From = this.ConversationReference.User;
                activity.Recipient = this.ConversationReference.Bot;
                activity.Conversation = this.ConversationReference.Conversation;
                activity.ServiceUrl = this.ConversationReference.ServiceUrl;

                var id = activity.Id = (this._nextId++).ToString();
                return this.OnReceive(activity);
            }
        }

        /// <summary>
        /// Send a message to the bot
        /// </summary>
        /// <param name="userSays"></param>
        /// <returns></returns>
        public TestFlow Send(string userSays)
        {
            return new TestFlow(this.SendActivityToBot(userSays), this);
        }

        /// <summary>
        /// Send an activity to the bot
        /// </summary>
        /// <param name="userSends"></param>
        /// <returns></returns>
        public TestFlow Send(Activity userSends)
        {
            return new TestFlow(this.SendActivityToBot(userSends), this);
        }

        /// <summary>
        /// Wait for period
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public TestFlow Delay(UInt32 ms)
        {
            return new TestFlow(Task.Delay((int)ms), this);
        }

        /// <summary>
        /// Assert that the reply matches expected
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReply(string expected, string description = null, UInt32 timeout = 3000)
        {
            return new TestFlow(Task.CompletedTask, this).AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Assert that the reply actiivty matches expected
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReply(Activity expected, string description = null, UInt32 timeout = 3000)
        {
            return new TestFlow(Task.CompletedTask, this).AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Custom validator for the reply activity
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReply(Action<IActivity> expected, string description = null, UInt32 timeout = 3000)
        {
            return new TestFlow(Task.CompletedTask, this).AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Assert that the reply is one of the candidates
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReplyOneOf(string[] candidates, string description = null, UInt32 timeout = 3000)
        {
            return new TestFlow(Task.CompletedTask, this).AssertReplyOneOf(candidates, description, timeout);
        }


        /// <summary>
        /// Say() -> shortcut for .Send(user).AssertReply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow Test(string userSays, string expected, string description = null, UInt32 timeout = 3000)
        {
            return new TestFlow(Task.CompletedTask, this).Send(userSays).AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Say() -> shortcut for .Send(user).AssertReply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow Test(string userSays, IActivity expected, string description = null, UInt32 timeout = 3000)
        {
            return new TestFlow(Task.CompletedTask, this).Send(userSays).AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Say() -> shortcut for .Send(user).AssertReply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow Test(string userSays, Action<IActivity> expected, string description = null, UInt32 timeout = 3000)
        {
            return new TestFlow(Task.CompletedTask, this).Send(userSays).AssertReply(expected, description, timeout);
        }
    }


    public class TestFlow
    {
        readonly TestAdapter _adapter;
        readonly Task testTask;


        public TestFlow(Task testTask, TestAdapter adapter)
        {
            this.testTask = testTask ?? Task.CompletedTask;
            this._adapter = adapter;
        }

        /// <summary>
        /// Start the execution of the test dialog
        /// </summary>
        /// <returns></returns>
        public Task StartTest()
        {
            return this.testTask;
        }

        /// <summary>
        /// Send a message from the user to the bot
        /// </summary>
        /// <param name="userSays"></param>
        /// <returns></returns>
        public TestFlow Send(string userSays)
        {
            if (userSays == null)
                throw new ArgumentNullException("You have to pass a userSays parameter");

            return new TestFlow(this.testTask.ContinueWith((task) =>
            {
                // NOTE: we need to .Wait() on the original Task to properly observe any exceptions that might have occurred
                // and to have them propagate correctly up through the chain to whomever is waiting on the parent task
                // The following StackOverflow answer provides some more details on why you want to do this: 
                // https://stackoverflow.com/questions/11904821/proper-way-to-use-continuewith-for-tasks/11906865#11906865
                //
                // From the Docs:
                //  https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/exception-handling-task-parallel-library
                //  Exceptions are propagated when you use one of the static or instance Task.Wait or Wait 
                //  methods, and you handle them by enclosing the call in a try/catch statement. If a task is the 
                //  parent of attached child tasks, or if you are waiting on multiple tasks, multiple exceptions 
                //  could be thrown.
                task.Wait();

                return this._adapter.SendActivityToBot(userSays);
            }).Unwrap(), this._adapter);
        }

        /// <summary>
        /// Send an activity from the user to the bot
        /// </summary>
        /// <param name="userActivity"></param>
        /// <returns></returns>
        public TestFlow Send(Activity userActivity)
        {
            if (userActivity == null)
                throw new ArgumentNullException("You have to pass an Activity");

            return new TestFlow(this.testTask.ContinueWith((task) =>
            {
                // NOTE: See details code in above method. 
                task.Wait();

                return this._adapter.SendActivityToBot(userActivity);
            }).Unwrap(), this._adapter);
        }

        /// <summary>
        /// Delay for time period 
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public TestFlow Delay(UInt32 ms)
        {
            return new TestFlow(this.testTask.ContinueWith((task) =>
            {
                // NOTE: See details code in above method. 
                task.Wait();

                return Task.Delay((int)ms);
            }).Unwrap(), this._adapter);
        }

        /// <summary>
        /// Assert that reply is expected text
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReply(string expected, string description = null, UInt32 timeout = 3000)
        {
            return this.AssertReply(this._adapter.MakeActivity(expected), description, timeout);
        }

        /// <summary>
        /// Assert that the reply is expected activity 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReply(IActivity expected, string description = null, UInt32 timeout = 3000)
        {
            return this.AssertReply((reply) =>
            {
                if (expected.Type != reply.Type)
                    throw new Exception($"{description}: Type should match");
                if (expected.AsMessageActivity().Text != reply.AsMessageActivity().Text)
                    throw new Exception($"{description}: Text should match");
                    // TODO, expand this to do all properties set on expected
                }, description, timeout);
        }

        /// <summary>
        /// Assert that the reply matches a custom validation routine
        /// </summary>
        /// <param name="validateActivity"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReply(Action<IActivity> validateActivity, string description, UInt32 timeout = 3000)
        {
            return new TestFlow(this.testTask.ContinueWith((task) =>
            {
                // NOTE: See details code in above method. 
                task.Wait();

                var start = DateTime.UtcNow;
                while (true)
                {
                    var current = DateTime.UtcNow;

                    if ((current - start).TotalMilliseconds > timeout)
                    {
                        throw new TimeoutException($"{timeout}ms Timed out waiting for:'{description}'");
                    }

                    IActivity replyActivity = this._adapter.GetNextReply();                    
                    if (replyActivity != null)     
                    {
                        // if we have a reply
                        validateActivity(replyActivity);
                        return;
                    }
                }
            }), this._adapter);
        }


        /// <summary>
        /// Say() -> shortcut for .Send(user).AssertReply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow Test(string userSays, string expected, string description = null, UInt32 timeout = 3000)
        {
            if (expected == null)
                throw new ArgumentNullException(nameof(expected));

            return this.Send(userSays)
                .AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Test() -> shortcut for .Send(user).AssertReply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow Test(string userSays, Activity expected, string description = null, UInt32 timeout = 3000)
        {
            if (expected == null)
                throw new ArgumentNullException(nameof(expected));

            return this.Send(userSays)
                .AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Say() -> shortcut for .Send(user).AssertReply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow Test(string userSays, Action<IActivity> expected, string description = null, UInt32 timeout = 3000)
        {
            if (expected == null)
                throw new ArgumentNullException(nameof(expected));

            return this.Send(userSays)
                .AssertReply(expected, description, timeout);
        }

        /// <summary>
        /// Assert that reply is one of the candidate responses
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReplyOneOf(string[] candidates, string description = null, UInt32 timeout = 3000)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            return this.AssertReply((reply) =>
            {
                foreach (var candidate in candidates)
                {
                    if (reply.AsMessageActivity().Text == candidate)
                        return;
                }
                throw new Exception(description ?? $"Not one of candidates: {String.Join("\n", candidates)}");
            }, description, timeout);
        }
    }
}
