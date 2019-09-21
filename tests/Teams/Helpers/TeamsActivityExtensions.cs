using System;
using System.Threading.Channels;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace Helpers
{
    public static class TeamsActivityExtensions
    {
        public static IMessageActivity NotifyUser(this IMessageActivity activity)
        {
            if (activity.ChannelData == null)
            {
                TeamsChannelData channelData = new TeamsChannelData
                {
                    Notification = new NotificationInfo
                    {
                        Alert = true,
                    },
                };

                activity.ChannelData = channelData;
            }
            else
            {
                TeamsChannelData channelData = activity.GetChannelData<TeamsChannelData>();
                channelData.Notification = new NotificationInfo
                {
                    Alert = true,
                };
                activity.ChannelData = channelData;
            }

            return activity;
        }

        public static ChannelInfo GetGeneralChannel(this IMessageActivity activity)
        {
            var channelData = activity.GetChannelData<TeamsChannelData>();

            if (channelData == null)
            {
                throw new ArgumentNullException("The Teams ChannelData object is null.");
            }

            if (channelData.Team == null)
            {
                throw new ArgumentNullException("The Team proprty in the ChannelData object is null");
            }

            return new ChannelInfo { Id = channelData.Team.Id };

        }

        public static IMessageActivity AddressMessageToTeamsGeneralChannel(this IMessageActivity activity)
        {
            var channelData = activity.GetChannelData<TeamsChannelData>();
            if (channelData == null)
            {
                throw new ArgumentNullException("The Teams ChannelData object is null.");
            }

            if (channelData.Team == null)
            {
                throw new ArgumentNullException("The Teams ChannelData Team properity is null.");
            }

            channelData.Channel = activity.GetGeneralChannel();

            if (activity.Conversation == null)
            {
                activity.Conversation = new ConversationAccount
                {
                    Id = channelData.Team.Id,
                    TenantId = channelData.Tenant.Id,
                    IsGroup = true,
                    ConversationType = "msteams",
                };
            }
            else
            {
                activity.Conversation.Id = channelData.Team.Id;
            }

            return activity;
        }
    }
}
