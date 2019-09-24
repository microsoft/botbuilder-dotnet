// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace Microsoft.Bot.Builder.Teams
{
    public static class TeamsActivityExtensions
    {
        public static void NotifyUser(this Activity activity)
        {
            var teamsChannelData = activity.ChannelData as TeamsChannelData;
            if (teamsChannelData == null)
            {
                teamsChannelData = new TeamsChannelData();
                activity.ChannelData = teamsChannelData;
            }

            teamsChannelData.Notification = new NotificationInfo
            {
                Alert = true,
            };
        }
    }
}
