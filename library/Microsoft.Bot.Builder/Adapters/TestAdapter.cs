using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters
{
    public class TestAdapter : ActivityAdapterBase
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

        public ConversationReference ConversationReference { get; set; }


        /// <summary>
        /// get next activity or null if none
        /// </summary>
        /// <returns></returns>
        public Activity GetNextReply()
        {
            lock (this.botReplies)
            {
                if (this.botReplies.Count > 0)
                    return this.botReplies.Dequeue();
            }
            return null;
        }

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
        /// Bot posting an activity back to the source
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task Post(IList<Activity> activities, CancellationToken token)
        {
            lock (this.botReplies)
            {
                foreach (var activity in activities)
                    this.botReplies.Enqueue(activity);
            }
        }

        /* INTERNAL */
        internal Task sendActivityToBot(string userSays)
        {
            return this.sendActivityToBot(this.MakeActivity(userSays));
        }

        internal Task sendActivityToBot(Activity activity)
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
                return this.OnReceive(activity, new CancellationToken());
            }
        }

        /// <summary>
        /// Send a message to the bot
        /// </summary>
        /// <param name="userSays"></param>
        /// <returns></returns>
        public TestFlow Send(string userSays)
        {
            return new TestFlow(this.sendActivityToBot(userSays), this);
        }

        /// <summary>
        /// Send an activity to the bot
        /// </summary>
        /// <param name="userSends"></param>
        /// <returns></returns>
        public TestFlow Send(Activity userSends)
        {
            return new TestFlow(this.sendActivityToBot(userSends), this);
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
        public TestFlow AssertReply(Action<Activity> expected, string description = null, UInt32 timeout = 3000)
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
        public TestFlow Test(string userSays, Activity expected, string description = null, UInt32 timeout = 3000)
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
        public TestFlow Test(string userSays, Action<Activity> expected, string description = null, UInt32 timeout = 3000)
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
                if (task.IsFaulted)
                    throw new Exception("failed");
                    return this._adapter.sendActivityToBot(userSays);
            }), this._adapter);
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
                if (task.IsFaulted)
                    throw new Exception("failed");
                return this._adapter.sendActivityToBot(userActivity);
            }), this._adapter);
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
                if (task.IsFaulted)
                    throw new Exception("failed");
                return Task.Delay((int)ms);
            }), this._adapter);
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
        public TestFlow AssertReply(Activity expected, string description = null, UInt32 timeout = 3000)
        {
            return this.AssertReply((reply) =>
            {
                if (expected.Type != reply.Type)
                    throw new Exception($"{description}: Type should match");
                if (expected.Text != reply.Text)
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
        public TestFlow AssertReply(Action<Activity> validateActivity, string description, UInt32 timeout = 3000)
        {
            return new TestFlow(this.testTask.ContinueWith((task) =>
            {
                if (task.IsFaulted)
                    throw new Exception("failed");
                var start = DateTime.UtcNow;
                while (true)
                {
                    var current = DateTime.UtcNow;

                    if ((current - start).TotalMilliseconds > timeout)
                    {
                        throw new TimeoutException($"{timeout}ms Timed out waiting for:${description}");
                    }

                    Activity replyActivity = this._adapter.GetNextReply();
                    // if we have a reply
                    if (replyActivity != null)
                    {
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
        public TestFlow Test(string userSays, Action<Activity> expected, string description = null, UInt32 timeout = 3000)
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
                    if (reply.Text == candidate)
                        return;
                }
                throw new Exception(description ?? $"Not one of candidates: {String.Join("\n", candidates)}");
            }, description, timeout);
        }
    }
}
