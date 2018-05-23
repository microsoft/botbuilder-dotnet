// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Testing
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

        public async Task ProcessActivity(Activity activity, Func<ITurnContext, Task> callback, CancellationTokenSource cancelToken = null)
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
            if (activity.Timestamp == null || activity.Timestamp == default(DateTime))
                activity.Timestamp = DateTime.UtcNow;

            using (var context = new TurnContext(this, activity))
            {
                await base.RunPipeline(context, callback, cancelToken);
            }
        }

        public ConversationReference ConversationReference { get; set; }


        public async override Task<ResourceResponse[]> SendActivities(ITurnContext context, Activity[] activities)
        {
            List<ResourceResponse> responses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                if (String.IsNullOrEmpty(activity.Id))
                    activity.Id = Guid.NewGuid().ToString("n");

                if (activity.Timestamp == null)
                    activity.Timestamp = DateTime.UtcNow;

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

    /// <summary>
    /// Called by TestFlow to validate and activity
    /// </summary>
    /// <param name="expected">Activity from trnascript file</param>
    /// <param name="actual">Activity from bot</param>
    public delegate void ValidateReply(IActivity expected, IActivity actual);
}
