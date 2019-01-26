// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    /// <summary>
    /// Representas a transcript logger that writes activites to a <see cref="Trace"/> object.
    /// </summary>
    public class SimpleChatLogger : ITranscriptLogger
    {
        /// <summary>
        /// Log an activity to the transcript.
        /// </summary>
        /// <param name="activity">The activity to transcribe.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public Task LogActivityAsync(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);
            if (activity.Type == ActivityTypes.Message)
            {
                System.Diagnostics.Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role}: {((Activity)activity).Text}");
            }
            else
            {
                System.Diagnostics.Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role} [{activity.Type}]");
            }
            return Task.CompletedTask;
        }
    }
}

