using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    public class TestConnector : Connector
    {
        private int _nextId = 0;
        //protected delegate void TestValidator(IList<Activity> activities);


        public TestConnector(ConversationReference reference = null)
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
        public Queue<Activity> BotReplies { get; set; } = new Queue<Activity>();

        public Activity MakeActivity(string text = null)
        {
            Activity a = new Activity
            {
                Type = ActivityTypes.Message,
                From = ConversationReference.User,
                Recipient = ConversationReference.Bot,
                Conversation = ConversationReference.Conversation,
                ServiceUrl = ConversationReference.ServiceUrl,
                Id = (_nextId++).ToString(),
                Text = text
            };

            //Attachments = Array.Empty<Attachment>(),
            //Entities = Array.Empty<Entity>(),

            return a;
        }

        /// <summary>
        /// Bot posting an activity back to the source
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task Post(IList<Activity> activities, CancellationToken token)
        {
            foreach (var activity in activities)
                this.BotReplies.Enqueue(activity);

        }

        /** INTERNAL implementation of `Connector.post()`. */
        public async Task<ReceiveResponse> Post(Activity[] activities)
        {
            lock (this.BotReplies)
            {
                foreach (var activity in activities)
                    this.BotReplies.Enqueue(activity);
            }

            return new ReceiveResponse() { Status = HttpStatusCode.OK };
        }

        /* INTERNAL */
        internal Task _sendActivityToBot(string userSays)
        {
            return this._sendActivityToBot(this.MakeActivity(userSays));
        }

        internal Task _sendActivityToBot(Activity activity)
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
            return this.Receive(activity, new CancellationToken());
        }

        /// <summary>
        /// Send a message to the bot
        /// </summary>
        /// <param name="userSays"></param>
        /// <returns></returns>
        public Test Send(string userSays)
        {
            return new Test(this._sendActivityToBot(userSays), this);
        }

        /// <summary>
        /// Send an activity to the bot
        /// </summary>
        /// <param name="userSends"></param>
        /// <returns></returns>
        public Test Send(Activity userSends)
        {
            return new Test(this._sendActivityToBot(userSends), this);
        }

        /// <summary>
        /// Wait for period
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Delay(int delay)
        {
            return new Test(Task.Delay(delay), this);
        }

        /// <summary>
        /// Assert that the reply matches expected
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Reply(string expected, string description = null, int delay = 3000)
        {
            return new Test(Task.CompletedTask, this).Reply(expected, description, delay);
        }

        /// <summary>
        /// Assert that the reply actiivty matches expected
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Reply(Activity expected, string description = null, int delay = 3000)
        {
            return new Test(Task.CompletedTask, this).Reply(expected, description, delay);
        }

        /// <summary>
        /// Custom validator for the reply activity
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Reply(Action<Activity> expected, string description = null, int delay = 3000)
        {
            return new Test(Task.CompletedTask, this).Reply(expected, description, delay);
        }

        /// <summary>
        /// Assert that the reply is one of the candidates
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test ReplyOneOf(string[] candidates, string description = null, int delay = 3000)
        {
            return new Test(Task.CompletedTask, this).ReplyOneOf(candidates, description, delay);
        }


        /// <summary>
        /// Say() -> shortcut for .Send(user).Reply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Say(string userSays, string expected, string description = null, int delay = 3000)
        {
            return new Test(Task.CompletedTask, this).Send(userSays).Reply(expected, description, delay);
        }

        /// <summary>
        /// Say() -> shortcut for .Send(user).Reply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Say(string userSays, Activity expected, string description = null, int delay = 3000)
        {
            return new Test(Task.CompletedTask, this).Send(userSays).Reply(expected, description, delay);
        }

        /// <summary>
        /// Say() -> shortcut for .Send(user).Reply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Say(string userSays, Action<Activity> expected, string description = null, int delay = 3000)
        {
            return new Test(Task.CompletedTask, this).Send(userSays).Reply(expected, description, delay);
        }
    }


    public class Test
    {
        TestConnector connector;
        Task previous;


        public Test(Task previous, TestConnector connector)
        {
            this.previous = previous ?? Task.CompletedTask;
            this.connector = connector;
        }

        /// <summary>
        /// Start the execution of the test dialog
        /// </summary>
        /// <returns></returns>
        public Task StartTest()
        {
            return this.previous;
        }

        /// <summary>
        /// Send a message from the user to the bot
        /// </summary>
        /// <param name="userSays"></param>
        /// <returns></returns>
        public Test Send(string userSays)
        {
            return new Test(this.previous.ContinueWith((task) =>
            {
                Assert.IsFalse(task.IsFaulted);
                return this.connector._sendActivityToBot(userSays);
            }), this.connector);
        }

        /// <summary>
        /// Send an activity from the user to the bot
        /// </summary>
        /// <param name="userActivity"></param>
        /// <returns></returns>
        public Test Send(Activity userActivity)
        {
            return new Test(this.previous.ContinueWith((task) =>
            {
                Assert.IsFalse(task.IsFaulted);
                return this.connector._sendActivityToBot(userActivity);
            }), this.connector);
        }

        /// <summary>
        /// Delay for time period 
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Delay(int delay)
        {
            return new Test(this.previous.ContinueWith((task) =>
            {
                Assert.IsFalse(task.IsFaulted);
                return Task.Delay(delay);
            }), this.connector);
        }

        /// <summary>
        /// Assert that reply is expected text
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Reply(string expected, string description = null, int delay = 3000)
        {
            return this.Reply(this.connector.MakeActivity(expected), description, delay);
        }

        /// <summary>
        /// Assert that the reply is expected activity 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Reply(Activity expected, string description = null, int delay = 3000)
        {
            return this.Reply((reply) =>
            {
                Assert.AreEqual(expected.Type, reply.Type, $"{description}: Type should match");
                Assert.AreEqual(expected.Text, reply.Text, $"{description}: Text should match");
                // TODO, expand this to do all properties set on expected
            }, description, delay);
        }

        /// <summary>
        /// Asser that the reply matches a custom validation routine
        /// </summary>
        /// <param name="validateActivity"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Reply(Action<Activity> validateActivity, string description, int delay = 3000)
        {
            return new Test(this.previous.ContinueWith((task) =>
            {
                Assert.IsFalse(task.IsFaulted);
                var start = DateTime.UtcNow;
                while (true)
                {
                    var current = DateTime.UtcNow;

                    if ((current - start).TotalMilliseconds > delay)
                    {
                        Assert.Fail($"{delay}ms Timed out waiting for:${description}");
                    }

                    Activity replyActivity = null;
                    lock (this.connector.BotReplies)
                    {
                        // if we have replies
                        if (this.connector.BotReplies.Count > 0)
                        {
                            replyActivity = this.connector.BotReplies.Dequeue();
                        }
                    }
                    if (replyActivity != null)
                    {
                        validateActivity(replyActivity);
                        return;
                    }
                }
            }), this.connector);
        }


        /// <summary>
        /// Say() -> shortcut for .Send(user).Reply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Say(string userSays, string expected, string description = null, int delay = 3000)
        {
            if (expected == null)
                throw new Exception(".say() Missing expected parameter");

            return this.Send(userSays)
                .Reply(expected, description, delay);
        }

        /// <summary>
        /// Say() -> shortcut for .Send(user).Reply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Say(string userSays, Activity expected, string description = null, int delay = 3000)
        {
            if (expected == null)
                throw new Exception(".say() Missing expected parameter");

            return this.Send(userSays)
                .Reply(expected, description, delay);
        }

        /// <summary>
        /// Say() -> shortcut for .Send(user).Reply(Expected)
        /// </summary>
        /// <param name="userSays"></param>
        /// <param name="expected"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test Say(string userSays, Action<Activity> expected, string description = null, int delay = 3000)
        {
            if (expected == null)
                throw new Exception(".say() Missing expected parameter");

            return this.Send(userSays)
                .Reply(expected, description, delay);
        }

        /// <summary>
        /// Assert that reply is one of the candidate responses
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="description"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Test ReplyOneOf(string[] candidates, string description = null, int delay = 3000)
        {
            if (candidates == null || candidates.Length == 0)
                throw new Exception(".replyOneOf() requires canidates");
            return this.Reply((reply) =>
            {
                foreach (var candidate in candidates)
                {
                    if (reply.Text == candidate)
                        return;
                }
                Assert.Fail(description ?? $"Not one of candidates: {String.Join("\n", candidates)}");
            }, description, delay);
        }
    }
}
