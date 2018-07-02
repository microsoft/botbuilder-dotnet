// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters
{
    /// <summary>
    /// A mock adapter that can be used for unit testing of bot logic.
    /// </summary>
    /// <seealso cref="TestFlow"/>
    public class TestAdapter : BotAdapter
    {
        private object _conversationLock = new object();
        private object _activeQueueLock = new object();

        private int _nextId = 0;

        public TestAdapter(ConversationReference conversation = null)
        {
            if (conversation != null)
            {
                Conversation = conversation;
            }
            else
            {
                Conversation = new ConversationReference
                {
                    ChannelId = "test",
                    ServiceUrl = "https://test.com"
                };

                Conversation.User = new ChannelAccount("user1", "User1");
                Conversation.Bot = new ChannelAccount("bot", "Bot");
                Conversation.Conversation = new ConversationAccount(false, "convo1", "Conversation1");
            }
        }

        public Queue<Activity> ActiveQueue { get; } = new Queue<Activity>();

        public new TestAdapter Use(IMiddleware middleware)
        {
            base.Use(middleware);
            return this;
        }

        public async Task ProcessActivity(Activity activity, Func<ITurnContext, Task> callback, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_conversationLock)
            {
                // ready for next reply
                if (activity.Type == null)
                    activity.Type = ActivityTypes.Message;
                activity.ChannelId = Conversation.ChannelId;
                activity.From = Conversation.User;
                activity.Recipient = Conversation.Bot;
                activity.Conversation = Conversation.Conversation;
                activity.ServiceUrl = Conversation.ServiceUrl;

                var id = activity.Id = (_nextId++).ToString();
            }
            if (activity.Timestamp == null || activity.Timestamp == default(DateTime))
                activity.Timestamp = DateTime.UtcNow;

            using (var context = new TurnContext(this, activity))
            {
                await RunPipeline(context, callback, cancellationToken);
            }
        }

        public ConversationReference Conversation { get; set; }


        public async override Task<ResourceResponse[]> SendActivities(ITurnContext context, Activity[] activities, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (activities == null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            if (activities.Length == 0)
            {
                throw new ArgumentException("Expecting one or more activities, but the array was empty.", nameof(activities));
            }

            var responses = new ResourceResponse[activities.Length];

            // NOTE: we're using for here (vs. foreach) because we want to simultaneously index into the
            // activities array to get the activity to process as well as use that index to assign
            // the response to the responses array and this is the most cost effective way to do that.
            for (var index = 0; index < activities.Length; index++)
            {
                var activity = activities[index];

                if (String.IsNullOrEmpty(activity.Id))
                {
                    activity.Id = Guid.NewGuid().ToString("n");
                }

                if (activity.Timestamp == null)
                {
                    activity.Timestamp = DateTime.UtcNow;
                }


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
                    lock (_activeQueueLock)
                    {
                        ActiveQueue.Enqueue(activity);
                    }
                }

                responses[index] = new ResourceResponse(activity.Id);
            }

            return responses;
        }

        public override Task<ResourceResponse> UpdateActivity(ITurnContext context, Activity activity, CancellationToken cancellationToken)
        {
            lock (_activeQueueLock)
            {
                var replies = ActiveQueue.ToList();
                for (int i = 0; i < ActiveQueue.Count; i++)
                {
                    if (replies[i].Id == activity.Id)
                    {
                        replies[i] = activity;
                        ActiveQueue.Clear();
                        foreach (var item in replies)
                        {
                            ActiveQueue.Enqueue(item);
                        }

                        return Task.FromResult(new ResourceResponse(activity.Id));
                    }
                }
            }

            return Task.FromResult(new ResourceResponse());
        }

        public override Task DeleteActivity(ITurnContext context, ConversationReference reference, CancellationToken cancellationToken)
        {
            lock (_activeQueueLock)
            {
                var replies = ActiveQueue.ToList();
                for (int i = 0; i < ActiveQueue.Count; i++)
                {
                    if (replies[i].Id == reference.ActivityId)
                    {
                        replies.RemoveAt(i);
                        ActiveQueue.Clear();
                        foreach (var item in replies)
                        {
                            ActiveQueue.Enqueue(item);
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
        //public override Task CreateConversation(string channelId, Func<ITurnContext, Task> callback)
        public Task CreateConversation(string channelId, Func<ITurnContext, Task> callback, CancellationToken cancellationToken)
        {
            ActiveQueue.Clear();
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
            lock (_activeQueueLock)
            {
                if (ActiveQueue.Count > 0)
                {
                    return ActiveQueue.Dequeue();
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
                From = Conversation.User,
                Recipient = Conversation.Bot,
                Conversation = Conversation.Conversation,
                ServiceUrl = Conversation.ServiceUrl,
                Id = (_nextId++).ToString(),
                Text = text
            };

            return activity;
        }


        /// <summary>
        /// Processes a message activity from a user.
        /// </summary>
        /// <param name="userSays">The text of the user's message.</param>
        /// <param name="callback">The turn processing logic to use.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="TestFlow.Send(string)"/>
        public Task SendTextToBot(string userSays, Func<ITurnContext, Task> callback, CancellationToken cancellationToken)
        {
            return ProcessActivity(MakeActivity(userSays), callback, cancellationToken);
        }
    }
}
