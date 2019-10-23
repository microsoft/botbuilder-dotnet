// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace Microsoft.Bot.Builder.Teams
{
    public static class TeamsActivityExtensions
    {
        public static string TeamsGetChannelId(this IActivity activity)
        {
            var channelData = activity.GetChannelData<TeamsChannelData>();
            return channelData?.Channel?.Id;
        }

        public static TeamInfo TeamsGetTeamInfo(this IActivity activity)
        {
            var channelData = activity.GetChannelData<TeamsChannelData>();
            return channelData?.Team;
        }

        public static void TeamsNotifyUser(this IActivity activity)
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
