// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace Microsoft.Bot.Builder.Teams
{
    /// <summary>
    /// The TeamsActivityExtensions
    /// provides helpers to make interacting with Microsoft Teams objects easier. 
    /// </summary>
    public static class TeamsActivityExtensions
    {
        /// <summary>
        /// Gets the TeamsMeetingInfo object from the current activity.
        /// </summary>
        /// <param name="activity">This activity.</param>
        /// <returns>The current activity's team's meeting, or null.</returns>
        public static TeamsMeetingInfo TeamsGetMeetingInfo(this IActivity activity)
        {
            var channelData = activity.GetChannelData<TeamsChannelData>();
            return channelData?.Meeting;
        }

        /// <summary>
        /// Gets the Team's channel id from the current activity.
        /// </summary>
        /// <param name="activity"> The current activity. </param>
        /// <returns>The current activity's team's channel, or empty string.</returns>
        public static string TeamsGetChannelId(this IActivity activity)
        {
            var channelData = activity.GetChannelData<TeamsChannelData>();
            return channelData?.Channel?.Id;
        }

        /// <summary>
        /// Gets the TeamsInfo object from the current activity.
        /// </summary>
        /// <param name="activity">This activity.</param>
        /// <returns>The current activity's team's Id, or an empty string.</returns>
        public static TeamInfo TeamsGetTeamInfo(this IActivity activity)
        {
            var channelData = activity.GetChannelData<TeamsChannelData>();
            return channelData?.Team;
        }

        /// <summary>
        /// Configures the current activity to generate a notification within Teams.
        /// </summary>
        /// <param name="activity">The current activity. </param>
        /// <param name="alertInMeeting">Sent to a meeting chat, this will cause the Teams client to 
        /// render it in a notification popup as well as in the chat thread.</param>
        /// <param name="externalResourceUrl">Url to external resource. Must be included in manifest's valid domains.</param>
        public static void TeamsNotifyUser(this IActivity activity, bool alertInMeeting, string externalResourceUrl = null)
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
                AlertInMeeting = alertInMeeting,
                ExternalResourceUrl = externalResourceUrl,
            };
        }

        /// <summary>
        /// Configures the current activity to generate a notification within Teams.
        /// </summary>
        /// <param name="activity">The current activity.</param>
        public static void TeamsNotifyUser(this IActivity activity)
        {
            activity.TeamsNotifyUser(false);
        }
    }
}
