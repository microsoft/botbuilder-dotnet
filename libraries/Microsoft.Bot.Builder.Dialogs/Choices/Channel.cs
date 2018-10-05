// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    public class Channel
    {
        public static bool SupportsSuggestedActions(string channelId, int buttonCnt = 100)
        {
            switch (channelId)
            {
                case Channels.Facebook:
                case Channels.Skype:
                    return buttonCnt <= 10;

                case Channels.Kik:
                    return buttonCnt <= 20;

                case Channels.Slack:
                case Channels.Telegram:
                case Channels.Emulator:
                    return buttonCnt <= 100;

                default:
                    return false;
            }
        }

        public static bool SupportsCardActions(string channelId, int buttonCnt = 100)
        {
            switch (channelId)
            {
                case Channels.Facebook:
                case Channels.Skype:
                case Channels.Msteams:
                    return buttonCnt <= 3;

                case Channels.Slack:
                case Channels.Emulator:
                case Channels.Directline:
                case Channels.Webchat:
                case Channels.Cortana:
                    return buttonCnt <= 100;

                default:
                    return false;
            }
        }

        public static bool HasMessageFeed(string channelId)
        {
            switch (channelId)
            {
                case Channels.Cortana:
                    return false;

                default:
                    return true;
            }
        }

        public static int MaxActionTitleLength(string channelId) => 20;

        public static string GetChannelId(ITurnContext turnContext) => string.IsNullOrEmpty(turnContext.Activity.ChannelId)
            ? string.Empty : turnContext.Activity.ChannelId;

        public class Channels : Connector.Channels
        {
        }
    }
}
