// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Skills.Internal
{
    internal interface ISkillResponseHandler
    {
        Task<ResourceResponse[]> OnSendActivitiesAsync(ITurnContext turnContext, List<Activity> activities, SendActivitiesHandler activitiesHandler, CancellationToken cancellationToken);

        Task<ResourceResponse> OnUpdateActivityAsync(ITurnContext turnContext, Activity activity, SendActivitiesHandler activitiesHandler, CancellationToken cancellationToken);

        Task OnDeleteActivityAsync(ITurnContext context, string activityId, SendActivitiesHandler activitiesHandler, CancellationToken cancellationToken);

        Activity GetEndOfConversationActivity();
    }
}
