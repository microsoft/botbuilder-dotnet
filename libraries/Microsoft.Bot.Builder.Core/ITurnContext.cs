// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public delegate Task SendActivitiesHandler(ITurnContext context, List<Activity> activities, Func<Task> next);
    public delegate Task UpdateActivityHandler(ITurnContext context, Activity activity, Func<Task> next);
    public delegate Task DeleteActivityHandler(ITurnContext context, ConversationReference reference, Func<Task> next);

    public interface ITurnContext
    {
        BotAdapter Adapter { get; }

        ITurnContextServiceCollection Services { get; }

        /// <summary>
        /// Incoming request
        /// </summary>
        Activity Activity { get; }

        /// <summary>
        /// 
        /// </summary>
        bool Responded { get; set; }

        Task<ResourceResponse> SendActivity(string textRepliesToSend, string speak = null, string inputHint = null);
        Task<ResourceResponse> SendActivity(IActivity activity);
        Task<ResourceResponse[]> SendActivities(IActivity[] activities);        

        Task<ResourceResponse> UpdateActivity(IActivity activity);

        Task DeleteActivity(string activityId);
        Task DeleteActivity(ConversationReference conversationReference);

        ITurnContext OnSendActivities(SendActivitiesHandler handler);
        ITurnContext OnUpdateActivity(UpdateActivityHandler handler);
        ITurnContext OnDeleteActivity(DeleteActivityHandler handler);
    }
}
