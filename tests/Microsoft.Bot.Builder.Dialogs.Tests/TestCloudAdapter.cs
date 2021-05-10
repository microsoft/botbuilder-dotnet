// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class TestCloudAdapter : CloudAdapterBase
    {
        public TestCloudAdapter(BotFrameworkAuthentication botFrameworkAuthentication)
            : base(botFrameworkAuthentication)
        {
        }

        public List<Activity> SentActivities { get; } = new List<Activity>();

        public Task<InvokeResponse> ProcessAsync(string authHeader, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken = default)
        {
            return ProcessActivityAsync(authHeader, activity, (tc, c) => callback(new InterceptTurnContext(this, tc), c), cancellationToken);
        }

        private class InterceptTurnContext : ITurnContext
        {
            private TestCloudAdapter _testAdapter;
            private ITurnContext _innerTurnContext;

            public InterceptTurnContext(TestCloudAdapter testAdapter, ITurnContext innerTurnContext)
            {
                _testAdapter = testAdapter;
                _innerTurnContext = innerTurnContext;
            }

            public BotAdapter Adapter => _innerTurnContext.Adapter;

            public TurnContextStateCollection TurnState => _innerTurnContext.TurnState;

            public Activity Activity => _innerTurnContext.Activity;

            public bool Responded => _innerTurnContext.Responded;

            public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
            {
                return _innerTurnContext.OnDeleteActivity(handler);
            }

            public ITurnContext OnSendActivities(SendActivitiesHandler handler)
            {
                return _innerTurnContext.OnSendActivities(handler);
            }

            public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
            {
                return _innerTurnContext.OnUpdateActivity(handler);
            }

            public Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default)
            {
                if (activity.Type != ActivityTypesEx.InvokeResponse)
                {
                    _testAdapter.SentActivities.Add((Activity)activity);
                    return Task.FromResult(new ResourceResponse());
                }
                else
                {
                    return _innerTurnContext.SendActivityAsync(activity, cancellationToken);
                }
            }

            public Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default)
            {
                _testAdapter.SentActivities.AddRange(activities.Cast<Activity>());
                return Task.FromResult(Enumerable.Repeat(new ResourceResponse(), activities.Length).ToArray());
            }

            public Task<ResourceResponse> SendActivityAsync(string textReplyToSend, string speak = null, string inputHint = "acceptingInput", CancellationToken cancellationToken = default)
            {
                return _innerTurnContext.SendActivityAsync(textReplyToSend, speak, inputHint, cancellationToken);
            }

            public Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default)
            {
                return _innerTurnContext.UpdateActivityAsync(activity, cancellationToken);
            }

            public Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default)
            {
                return _innerTurnContext.DeleteActivityAsync(activityId, cancellationToken);
            }

            public Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default)
            {
                return _innerTurnContext.DeleteActivityAsync(conversationReference, cancellationToken);
            }
        }
    }
}
