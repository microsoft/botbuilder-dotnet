// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using System.Threading;

namespace Microsoft.Bot.Builder.Adapters
{
    public class TestAdapter : BotAdapter
    {
        private int _nextId = 0;
        private readonly Queue<Activity> botReplies = new Queue<Activity>();

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

        public Queue<Activity> ActiveQueue { get { return botReplies; } }

        public new TestAdapter Use(IMiddleware middleware)
        {
            base.Use(middleware);
            return this;
        }

        public Task ProcessActivity(Activity activity, Func<ITurnContext, Task> callback, CancellationTokenSource cancelToken = null)
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
            }

            var context = new TurnContext(this, activity);
            return base.RunPipeline(context, callback, cancelToken);
        }

        public ConversationReference ConversationReference { get; set; }


        public async override Task<ResourceResponse[]> SendActivities(ITurnContext context, Activity[] activities)
        {
            List<ResourceResponse> responses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {                
                responses.Add(new ResourceResponse(activity.Id));

                if (activity.Type == ActivityTypesEx.Delay)
                {
                    // The BotFrameworkAdapter and Console adapter implement this
                    // hack directly in the POST method. Replicating that here
                    // to keep the behavior as close as possible to facillitate
                    // more realistic tests.                     
                    int delayMs = (int)activity.Value;
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

            return responses.ToArray();
        }

        public override Task<ResourceResponse> UpdateActivity(ITurnContext context, Activity activity)
        {
            lock (this.botReplies)
            {
                var replies = this.botReplies.ToList();
                for (int i = 0; i < this.botReplies.Count; i++)
                {
                    if (replies[i].Id == activity.Id)
                    {
                        replies[i] = activity;
                        this.botReplies.Clear();
                        foreach (var item in replies)
                        {
                            this.botReplies.Enqueue(item);
                        }
                        return Task.FromResult(new ResourceResponse(activity.Id));
                    }
                }
            }
            return Task.FromResult(new ResourceResponse());
        }

        public override Task DeleteActivity(ITurnContext context, ConversationReference reference)
        {
            lock (this.botReplies)
            {
                var replies = this.botReplies.ToList();
                for (int i = 0; i < this.botReplies.Count; i++)
                {
                    if (replies[i].Id == reference.ActivityId)
                    {
                        replies.RemoveAt(i);
                        this.botReplies.Clear();
                        foreach (var item in replies)
                        {
                            this.botReplies.Enqueue(item);
                        }
                        break;
                    }
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// NOTE: this resets the queue, it doesn't actually maintain multiple converstion queues
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public override Task CreateConversation(string channelId, Func<ITurnContext, Task> callback)
        {
            this.ActiveQueue.Clear();
            var update = Activity.CreateConversationUpdateActivity();
            update.Conversation = new ConversationAccount() { Id = Guid.NewGuid().ToString("n") };
            var context = new TurnContext(this, (Activity)update);
            return callback(context);
        }

        /// <summary>
        /// Called by TestFlow to check next reply
        /// </summary>
        /// <returns></returns>
        public IActivity GetNextReply()
        {
            lock (this.botReplies)
            {
                if (this.botReplies.Count > 0)
                {
                    return this.botReplies.Dequeue();
                }
            }
            return null;
        }

        /// <summary>
        /// Called by TestFlow to get appropriate activity for conversationReference of testbot
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Activity MakeActivity(string text = null)
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
        /// Called by TestFlow to send text to the bot
        /// </summary>
        /// <param name="userSays"></param>
        /// <returns></returns>
        public Task SendTextToBot(string userSays, Func<ITurnContext, Task> callback)
        {
            return this.ProcessActivity(this.MakeActivity(userSays), callback);
        }
    }


    public class TestFlow
    {
        readonly TestAdapter adapter;
        readonly Task testTask;
        Func<ITurnContext, Task> callback;

        public TestFlow(TestAdapter adapter, Func<ITurnContext, Task> callback = null)
        {
            this.adapter = adapter;
            this.callback = callback;
            this.testTask = testTask ?? Task.CompletedTask;
        }

        public TestFlow(Task testTask, TestFlow flow)
        {
            this.testTask = testTask ?? Task.CompletedTask;
            this.callback = flow.callback;
            this.adapter = flow.adapter;
        }

        /// <summary>
        /// Start the execution of the test flow
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

                return this.adapter.SendTextToBot(userSays, this.callback);
            }).Unwrap(), this);
        }

        /// <summary>
        /// Send an activity from the user to the bot
        /// </summary>
        /// <param name="userActivity"></param>
        /// <returns></returns>
        public TestFlow Send(IActivity userActivity)
        {
            if (userActivity == null)
                throw new ArgumentNullException("You have to pass an Activity");

            return new TestFlow(this.testTask.ContinueWith((task) =>
            {
                // NOTE: See details code in above method. 
                task.Wait();

                return this.adapter.ProcessActivity((Activity)userActivity, this.callback);
            }).Unwrap(), this);
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
            }), this);
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
            return this.AssertReply(this.adapter.MakeActivity(expected), description, timeout);
        }

        /// <summary>
        /// Assert that the reply is expected activity 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TestFlow AssertReply(IActivity expected, [CallerMemberName] string description = null, UInt32 timeout = 3000)
        {
            return this.AssertReply((reply) =>
            {
                if (expected.Type != reply.Type)
                    throw new Exception($"{description}: Type should match");
                if (expected.AsMessageActivity().Text != reply.AsMessageActivity().Text)
                {
                    if (description == null)
                        throw new Exception($"Expected:{expected.AsMessageActivity().Text}\nReceived:{reply.AsMessageActivity().Text}");
                    else
                        throw new Exception($"{description}: Text should match");
                }
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
        public TestFlow AssertReply(Action<IActivity> validateActivity, [CallerMemberName] string description = null, UInt32 timeout = 3000)
        {
            return new TestFlow(this.testTask.ContinueWith((task) =>
            {
                // NOTE: See details code in above method. 
                task.Wait();

                if (System.Diagnostics.Debugger.IsAttached)
                    timeout = UInt32.MaxValue;

                var start = DateTime.UtcNow;
                while (true)
                {
                    var current = DateTime.UtcNow;

                    if ((current - start).TotalMilliseconds > timeout)
                    {
                        throw new TimeoutException($"{timeout}ms Timed out waiting for:'{description}'");
                    }

                    IActivity replyActivity = this.adapter.GetNextReply();
                    if (replyActivity != null)
                    {
                        // if we have a reply
                        validateActivity(replyActivity);
                        return;
                    }
                }
            }), this);
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
