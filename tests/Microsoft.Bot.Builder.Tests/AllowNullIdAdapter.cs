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
    /// A mock adapter that can be used for unit testing bot logic.
    /// Allows for activity IDs to remain null in <seealso cref="AllowNullIdAdapter.SendActivitiesAsync(turnContext, activities, cancellationToken)"/>.
    /// </summary>
    public class AllowNullIdAdapter : TestAdapter
    {
        private bool _sendTraceActivity;
        private readonly object _activeQueueLock = new object();
        private Queue<TaskCompletionSource<IActivity>> _queuedRequests = new Queue<TaskCompletionSource<IActivity>>();

        public AllowNullIdAdapter(ConversationReference conversation = null, bool sendTraceActivity = false)
            : base(conversation, sendTraceActivity)
            { 
                _sendTraceActivity = sendTraceActivity;
            }
        
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
                        Enqueue(activity);
                    }
                }
                else
                {
                    Enqueue(activity);
                }

                responses[index] = new ResourceResponse(activity.Id);
            }

            return responses;
        }

        private void Enqueue(Activity activity)
        {
            lock (_activeQueueLock)
            {
                // if there are pending requests, fulfill them with the activity.
                while (_queuedRequests.Any())
                {
                    var tcs = _queuedRequests.Dequeue();
                    if (tcs.Task.IsCanceled == false)
                    {
                        tcs.SetResult(activity);
                        return;
                    }
                }

                // else we enqueue for next requester
                ActiveQueue.Enqueue(activity);
            }
        }
    }
}