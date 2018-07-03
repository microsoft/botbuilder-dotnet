// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// When added, this middleware will log incoming and outgoing activitites to a ITranscriptStore.
    /// </summary>
    public class TranscriptLoggerMiddleware : IMiddleware
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        private ITranscriptLogger logger;

        private Queue<IActivity> transcript = new Queue<IActivity>();

        /// <summary>
        /// Middleware for logging incoming and outgoing activities to a transcript store.
        /// </summary>
        /// <param name="transcriptLogger">The transcript logger to use.</param>
        public TranscriptLoggerMiddleware(ITranscriptLogger transcriptLogger)
        {
            logger = transcriptLogger ?? throw new ArgumentNullException("TranscriptLoggerMiddleware requires a ITranscriptLogger implementation.  ");
        }

        /// <summary>
        /// initialization for middleware turn.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="nextTurn"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task OnTurn(ITurnContext context, NextDelegate nextTurn, CancellationToken cancellationToken)
        {
            // log incoming activity at beginning of turn
            if (context.Activity != null)
            {
                if (string.IsNullOrEmpty((string)context.Activity.From.Properties["role"]))
                {
                    context.Activity.From.Properties["role"] = "user";
                }

                LogActivity(CloneActivity(context.Activity));
            }

            // hook up onSend pipeline
            context.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                // run full pipeline
                var responses = await nextSend().ConfigureAwait(false);

                foreach (var activity in activities)
                {
                    LogActivity(CloneActivity(activity));
                }

                return responses;
            });

            // hook up update activity pipeline
            context.OnUpdateActivity(async (ctx, activity, nextUpdate) =>
            {
                // run full pipeline
                var response = await nextUpdate().ConfigureAwait(false);

                // add Message Update activity
                var updateActivity = CloneActivity(activity);
                updateActivity.Type = ActivityTypes.MessageUpdate;
                LogActivity(updateActivity);
                return response;
            });

            // hook up delete activity pipeline
            context.OnDeleteActivity(async (ctx, reference, nextDelete) =>
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

                LogActivity(deleteActivity);
            });

            // process bot logic
            await nextTurn(cancellationToken).ConfigureAwait(false);

            // flush transcript at end of turn
            while (transcript.Count > 0)
            {
                try
                {
                    var activity = transcript.Dequeue();
                    await logger.LogActivity(activity).ConfigureAwait(false);
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.TraceError($"Transcript logActivity failed with {err}");
                }
            }
        }

        private static IActivity CloneActivity(IActivity activity)
        {
            activity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity, JsonSettings));
            return activity;
        }

        private void LogActivity(IActivity activity)
        {
            lock (transcript)
            {
                if (activity.Timestamp == null)
                {
                    activity.Timestamp = DateTime.UtcNow;
                }

                transcript.Enqueue(activity);
            }
        }
    }
}
