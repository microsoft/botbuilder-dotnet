﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters
{
    /// <summary>
    /// A mock adapter that can be used for unit testing of bot logic.
    /// </summary>
    /// <seealso cref="TestFlow"/>
    public class TestAdapter : BotAdapter, IUserTokenProvider
    {
        private readonly bool _sendTraceActivity;
        private readonly object _conversationLock = new object();
        private readonly object _activeQueueLock = new object();
        private readonly IDictionary<UserTokenKey, string> _userTokens = new Dictionary<UserTokenKey, string>();
        private readonly IList<TokenMagicCode> _magicCodes = new List<TokenMagicCode>();

        private int _nextId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAdapter"/> class.
        /// </summary>
        /// <param name="channelId">The target <see cref="Channels"/> for the test the will be passed to the bot.</param>
        /// <param name="sendTraceActivity">Indicates whether the adapter should add to its <see cref="ActiveQueue"/>
        /// any trace activities generated by the bot.</param>
        public TestAdapter(string channelId, bool sendTraceActivity = false)
        {
            _sendTraceActivity = sendTraceActivity;

            Conversation = new ConversationReference
            {
                ChannelId = channelId,
                ServiceUrl = "https://test.com",
                User = new ChannelAccount("user1", "User1"),
                Bot = new ChannelAccount("bot", "Bot"),
                Conversation = new ConversationAccount(false, "convo1", "Conversation1"),
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAdapter"/> class.
        /// </summary>
        /// <param name="conversation">A reference to the conversation to begin the adapter state with.</param>
        /// <param name="sendTraceActivity">Indicates whether the adapter should add to its <see cref="ActiveQueue"/>
        /// any trace activities generated by the bot.</param>
        public TestAdapter(ConversationReference conversation = null, bool sendTraceActivity = false)
        {
            _sendTraceActivity = sendTraceActivity;
            if (conversation != null)
            {
                Conversation = conversation;
            }
            else
            {
                Conversation = new ConversationReference
                {
                    ChannelId = Channels.Test,
                    ServiceUrl = "https://test.com",
                    User = new ChannelAccount("user1", "User1"),
                    Bot = new ChannelAccount("bot", "Bot"),
                    Conversation = new ConversationAccount(false, "convo1", "Conversation1"),
                };
            }
        }

        public string Locale { get; set; } = "en-us";

        /// <summary>
        /// Gets the queue of responses from the bot.
        /// </summary>
        /// <value>The queue of responses from the bot.</value>
        public Queue<Activity> ActiveQueue { get; } = new Queue<Activity>();

        /// <summary>
        /// Gets or sets a reference to the current conversation.
        /// </summary>
        /// <value>A reference to the current conversation.</value>
        public ConversationReference Conversation { get; set; }

        /// <summary>
        /// Create a ConversationReference. 
        /// </summary>
        /// <param name="name">name of the conversation (also id).</param>
        /// <param name="user">name of the user (also id) default:User1.</param>
        /// <param name="bot">name of the bot (also id) default:Bot.</param>
        /// <returns>ConversationReference.</returns>
        public static ConversationReference CreateConversation(string name, string user = "User1", string bot = "Bot")
        {
            return new ConversationReference
            {
                ChannelId = "test",
                ServiceUrl = "https://test.com",
                Conversation = new ConversationAccount(false, name, name),
                User = new ChannelAccount(id: user.ToLower(), name: user),
                Bot = new ChannelAccount(id: bot.ToLower(), name: bot),
            };
        }

        /// <summary>
        /// Adds middleware to the adapter's pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        /// <returns>The updated adapter object.</returns>
        /// <remarks>Middleware is added to the adapter at initialization time.
        /// For each turn, the adapter calls middleware in the order in which you added it.
        /// </remarks>
        public new TestAdapter Use(IMiddleware middleware)
        {
            base.Use(middleware);
            return this;
        }

        /// <summary>
        /// Receives an activity and runs it through the middleware pipeline.
        /// </summary>
        /// <param name="activity">The activity to process.</param>
        /// <param name="callback">The bot logic to invoke.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task ProcessActivityAsync(Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_conversationLock)
            {
                // ready for next reply
                if (activity.Type == null)
                {
                    activity.Type = ActivityTypes.Message;
                }

                activity.ChannelId = Conversation.ChannelId;
                activity.From = Conversation.User;
                activity.Recipient = Conversation.Bot;
                activity.Conversation = Conversation.Conversation;
                activity.ServiceUrl = Conversation.ServiceUrl;

                var id = activity.Id = (_nextId++).ToString();
            }

            if (activity.Timestamp == null || activity.Timestamp == default(DateTimeOffset))
            {
                activity.Timestamp = DateTime.UtcNow;
            }

            using (var context = new TurnContext(this, activity))
            {
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a turn context and runs the middleware pipeline for an incoming activity.
        /// </summary>
        /// <param name="identity">A <see cref="ClaimsIdentity"/> for the request.</param>
        /// <param name="activity">The incoming activity.</param>
        /// <param name="callback">The code to run at the end of the adapter's middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task<InvokeResponse> ProcessActivityAsync(ClaimsIdentity identity, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            await ProcessActivityAsync(activity, callback, cancellationToken).ConfigureAwait(false);
            return null;
        }

        /// <summary>
        /// Sends activities to the conversation.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="activities">The activities to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// an array of <see cref="ResourceResponse"/> objects containing the IDs that
        /// the receiving channel assigned to the activities.</remarks>
        /// <seealso cref="ITurnContext.OnSendActivities(SendActivitiesHandler)"/>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
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

                if (string.IsNullOrEmpty(activity.Id))
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
                    // to keep the behavior as close as possible to facilitate
                    // more realistic tests.
                    var delayMs = (int)activity.Value;

                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
                else if (activity.Type == ActivityTypes.Trace)
                {
                    if (_sendTraceActivity)
                    {
                        lock (_activeQueueLock)
                        {
                            ActiveQueue.Enqueue(activity);
                        }
                    }
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

        /// <summary>
        /// Replaces an existing activity in the <see cref="ActiveQueue"/>.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="activity">New replacement activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving
        /// channel assigned to the activity.
        /// <para>Before calling this, set the ID of the replacement activity to the ID
        /// of the activity to replace.</para></remarks>
        /// <seealso cref="ITurnContext.OnUpdateActivity(UpdateActivityHandler)"/>
        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
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

        /// <summary>
        /// Deletes an existing activity in the <see cref="ActiveQueue"/>.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="reference">Conversation reference for the activity to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>The <see cref="ConversationReference.ActivityId"/> of the conversation
        /// reference identifies the activity to delete.</remarks>
        /// <seealso cref="ITurnContext.OnDeleteActivity(DeleteActivityHandler)"/>
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
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
        /// Creates a new conversation on the specified channel.
        /// </summary>
        /// <param name="channelId">The ID of the channel.</param>
        /// <param name="callback">The bot logic to call when the conversation is created.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This resets the <see cref="ActiveQueue"/>, and does not maintain multiple conversation queues.</remarks>
        public Task CreateConversationAsync(string channelId, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            ActiveQueue.Clear();
            var update = Activity.CreateConversationUpdateActivity();
            update.Conversation = new ConversationAccount() { Id = Guid.NewGuid().ToString("n") };
            var context = new TurnContext(this, (Activity)update);
            return callback(context, cancellationToken);
        }

        /// <summary>
        /// Dequeues and returns the next bot response from the <see cref="ActiveQueue"/>.
        /// </summary>
        /// <returns>The next activity in the queue; or null, if the queue is empty.</returns>
        /// <remarks>A <see cref="TestFlow"/> object calls this to get the next response from the bot.</remarks>
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
        /// Creates a message activity from text and the current conversational context.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <returns>An appropriate message activity.</returns>
        /// <remarks>A <see cref="TestFlow"/> object calls this to get a message activity
        /// appropriate to the current conversation.</remarks>
        public Activity MakeActivity(string text = null)
        {
            Activity activity = new Activity
            {
                Type = ActivityTypes.Message,
                Locale = this.Locale,
                From = Conversation.User,
                Recipient = Conversation.Bot,
                Conversation = Conversation.Conversation,
                ServiceUrl = Conversation.ServiceUrl,
                Id = (_nextId++).ToString(),
                Text = text,
            };

            return activity;
        }

        /// <summary>
        /// Processes a message activity from a user.
        /// </summary>
        /// <param name="userSays">The text of the user's message.</param>
        /// <param name="callback">The turn processing logic to use.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="TestFlow.Send(string)"/>
        public virtual Task SendTextToBotAsync(string userSays, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return ProcessActivityAsync(MakeActivity(userSays), callback, cancellationToken);
        }

        /// <summary>
        /// Adds a fake user token so it can later be retrieved.
        /// </summary>
        /// <param name="connectionName">The connection name.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="token">The token to store.</param>
        /// <param name="magicCode">The optional magic code to associate with this token.</param>
        /// <param name="serviceProvider">The optional service provider name. Default same as the connection name.</param>
        public void AddUserToken(string connectionName, string channelId, string userId, string token, string magicCode = null, string serviceProvider = null)
        {
            var key = new UserTokenKey()
            {
                ConnectionName = connectionName,
                ServiceProviderDisplayName = serviceProvider ?? connectionName,
                ChannelId = channelId,
                UserId = userId,
            };

            if (magicCode == null)
            {
                if (_userTokens.ContainsKey(key))
                {
                    _userTokens[key] = token;
                }
                else
                {
                    _userTokens.Add(key, token);
                }
            }
            else
            {
                _magicCodes.Add(new TokenMagicCode()
                {
                    Key = key,
                    MagicCode = magicCode,
                    UserToken = token,
                });
            }
        }

        /// <summary>Attempts to retrieve the token for a user that's in a login flow.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="magicCode">(Optional) Optional user entered code to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Token Response or null if the token was not found.</returns>
        public virtual Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, string connectionName, string magicCode, CancellationToken cancellationToken)
        {
            var key = new UserTokenKey()
            {
                ConnectionName = connectionName,
                ChannelId = turnContext.Activity.ChannelId,
                UserId = turnContext.Activity.From.Id,
            };

            if (magicCode != null)
            {
                var magicCodeRecord = _magicCodes.FirstOrDefault(x => key.Equals(x.Key));
                if (magicCodeRecord != null && magicCodeRecord.MagicCode == magicCode)
                {
                    // move the token to long term dictionary
                    AddUserToken(connectionName, key.ChannelId, key.UserId, magicCodeRecord.UserToken);
                    _magicCodes.Remove(magicCodeRecord);
                }
            }

            if (_userTokens.TryGetValue(key, out string token))
            {
                // found
                return Task.FromResult(new TokenResponse()
                {
                    ConnectionName = connectionName,
                    Token = token,
                });
            }
            else
            {
                // not found
                return Task.FromResult<TokenResponse>(null);
            }
        }

        /// <summary>
        /// Returns a fake link for a sign-in.
        /// </summary>
        /// <param name="turnContext">The turn context (must have a valid Activity).</param>
        /// <param name="connectionName">The connectionName.</param>
        /// <param name="cancellationToken">A Task cancellationToken.</param>
        /// <returns>The signin link.</returns>
        public virtual Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, string connectionName, CancellationToken cancellationToken)
        {
            return GetOauthSignInLinkAsync(turnContext, connectionName, turnContext.Activity.From.Id, null, cancellationToken);
        }

        /// <summary>
        /// Returns a fake link for a sign-in.
        /// </summary>
        /// <param name="turnContext">The turn context (must have a valid Activity).</param>
        /// <param name="connectionName">The connectionName.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="finalRedirect">The final redirect value, which is ignored here.</param>
        /// <param name="cancellationToken">A Task cancellationToken.</param>
        /// <returns>The signin link.</returns>
        public virtual Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, string connectionName, string userId, string finalRedirect = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult($"https://fake.com/oauthsignin/{connectionName}/{turnContext.Activity.ChannelId}/{userId}");
        }

        /// <summary>
        /// Signs a user out by remove the user's token(s) from mock storage.
        /// </summary>
        /// <param name="turnContext">The turnContext (with a valid Activity).</param>
        /// <param name="connectionName">The conectionName.</param>
        /// <param name="userId">The userId.</param>
        /// <param name="cancellationToken">The Task cancellation token.</param>
        /// <returns>None.</returns>
        public virtual Task SignOutUserAsync(ITurnContext turnContext, string connectionName = null, string userId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var channelId = turnContext.Activity.ChannelId;
            userId = userId ?? turnContext.Activity.From.Id;

            var records = _userTokens.ToArray();
            foreach (var t in records)
            {
                if (t.Key.ChannelId == channelId &&
                    t.Key.UserId == userId &&
                    (connectionName == null || connectionName == t.Key.ConnectionName))
                {
                    _userTokens.Remove(t.Key);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the token statuses.
        /// </summary>
        /// <param name="context">The turnContext (with a valid Activity).</param>
        /// <param name="userId">The user id.</param>
        /// <param name="includeFilter">Optional comma separated list of connection's to include. Blank will return token status for all configured connections.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Array of TokenStatus.</returns>
        public virtual Task<TokenStatus[]> GetTokenStatusAsync(ITurnContext context, string userId, string includeFilter = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = includeFilter == null ? null : includeFilter.Split(',');
            var records = _userTokens.
                Where(x =>
                    x.Key.ChannelId == context.Activity.ChannelId &&
                    x.Key.UserId == context.Activity.From.Id &&
                    (includeFilter == null || filter.Contains(x.Key.ConnectionName))).
                Select(r => new TokenStatus() { ChannelId = r.Key.ChannelId, ConnectionName = r.Key.ConnectionName, HasToken = true, ServiceProviderDisplayName = r.Key.ServiceProviderDisplayName }).ToArray();

            if (records.Any())
            {
                return Task.FromResult(records);
            }

            return Task.FromResult<TokenStatus[]>(null);
        }

        /// <summary>
        /// Returns a dictionary of TokenResponses for the resource URLs.
        /// </summary>
        /// <param name="context">The TurnContext.</param>
        /// <param name="connectionName">The connectionName.</param>
        /// <param name="resourceUrls">The list of AAD resource URLs.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The dictionary of TokenResponses for each resource URL.</returns>
        public virtual Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(ITurnContext context, string connectionName, string[] resourceUrls, string userId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(new Dictionary<string, TokenResponse>());
        }

        private class UserTokenKey
        {
            public string ConnectionName { get; set; }

            public string ServiceProviderDisplayName { get; set; }

            public string UserId { get; set; }

            public string ChannelId { get; set; }

            public override bool Equals(object obj)
            {
                var rhs = obj as UserTokenKey;
                if (rhs != null)
                {
                    return string.Equals(this.ConnectionName, rhs.ConnectionName) &&
                        string.Equals(this.UserId, rhs.UserId) &&
                        string.Equals(this.ChannelId, rhs.ChannelId);
                }

                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return (ConnectionName ?? string.Empty).GetHashCode() +
                    (UserId ?? string.Empty).GetHashCode() +
                    (ChannelId ?? string.Empty).GetHashCode();
            }
        }

        private class TokenMagicCode
        {
            public UserTokenKey Key { get; set; }

            public string MagicCode { get; set; }

            public string UserToken { get; set; }
        }
    }
}
