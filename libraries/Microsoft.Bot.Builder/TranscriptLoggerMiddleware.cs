﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Middleware for logging incoming and outgoing activities to an <see cref="ITranscriptStore"/>.
    /// </summary>
    public class TranscriptLoggerMiddleware : IMiddleware
    {
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        private readonly ITranscriptLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranscriptLoggerMiddleware"/> class.
        /// </summary>
        /// <param name="transcriptLogger">The conversation store to use.</param>
        public TranscriptLoggerMiddleware(ITranscriptLogger transcriptLogger)
        {
            _logger = transcriptLogger ?? throw new ArgumentNullException(nameof(transcriptLogger), "TranscriptLoggerMiddleware requires a ITranscriptLogger implementation.  ");
        }

        /// <summary>
        /// Records incoming and outgoing activities to the conversation store.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="nextTurn">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate nextTurn, CancellationToken cancellationToken)
        {
            Queue<IActivity> transcript = new Queue<IActivity>();

            // log incoming activity at beginning of turn
            if (turnContext.Activity != null)
            {
                if (turnContext.Activity.From == null)
                {
                    turnContext.Activity.From = new ChannelAccount();
                }

                if (string.IsNullOrEmpty((string)turnContext.Activity.From.Properties["role"]))
                {
                    turnContext.Activity.From.Properties["role"] = "user";
                }

                LogActivity(transcript, CloneActivity(turnContext.Activity));
            }

            // hook up onSend pipeline
            turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                // run full pipeline
                var responses = await nextSend().ConfigureAwait(false);

                foreach (var activity in activities)
                {
                    LogActivity(transcript, CloneActivity(activity));
                }

                return responses;
            });

            // hook up update activity pipeline
            turnContext.OnUpdateActivity(async (ctx, activity, nextUpdate) =>
            {
                // run full pipeline
                var response = await nextUpdate().ConfigureAwait(false);

                // add Message Update activity
                var updateActivity = CloneActivity(activity);
                updateActivity.Type = ActivityTypes.MessageUpdate;
                LogActivity(transcript, updateActivity);
                return response;
            });

            // hook up delete activity pipeline
            turnContext.OnDeleteActivity(async (ctx, reference, nextDelete) =>
            {
                // run full pipeline
                await nextDelete().ConfigureAwait(false);

                // add MessageDelete activity
                // log as MessageDelete activity
                var deleteActivity = new Activity
                {
                    Type = ActivityTypes.MessageDelete,
                    Id = reference.ActivityId,
                }
                .ApplyConversationReference(reference, isIncoming: false)
                .AsMessageDeleteActivity();

                LogActivity(transcript, deleteActivity);
            });

            // process bot logic
            await nextTurn(cancellationToken).ConfigureAwait(false);

            // flush transcript at end of turn
            // NOTE: We are not awaiting this task by design, TryLogTranscriptAsync() observes all exceptions and we don't need to or want to block execution on the completion.
            _ = TryLogTranscriptAsync(_logger, transcript);
        }

        /// <summary>
        /// Helper to sequentially flush the transcript queue to the log.
        /// </summary>
        private static async Task TryLogTranscriptAsync(ITranscriptLogger logger, Queue<IActivity> transcript)
        {
            try
            {
                while (transcript.Count > 0)
                {
                    // Process the queue and log all the activities in parallel.
                    var activity = transcript.Dequeue();
                    await logger.LogActivityAsync(activity).ConfigureAwait(false);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types (this should probably be addressed later, but for now we just log the error and continue the execution)
            catch (Exception ex)
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
            {
                Trace.TraceError($"Transcript logActivity failed with {ex}");
            }
        }

        private static IActivity CloneActivity(IActivity activity)
        {
            activity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity, _jsonSettings));
            var activityWithId = EnsureActivityHasId(activity);

            return activityWithId;
        }

        private static IActivity EnsureActivityHasId(IActivity activity)
        {
            var activityWithId = activity;

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity), "Cannot check or add Id on a null Activity.");
            }

            if (activity.Id == null)
            {
                var generatedId = $"g_{Guid.NewGuid()}";
                activity.Id = generatedId;
            }

            return activityWithId;
        }

        private static void LogActivity(Queue<IActivity> transcript, IActivity activity)
        {
            if (activity.Timestamp == null)
            {
                activity.Timestamp = DateTime.UtcNow;
            }

            transcript.Enqueue(activity);
        }
    }
}
