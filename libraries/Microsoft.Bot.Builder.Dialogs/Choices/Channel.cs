// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    public class Channel
    {
        public static bool SupportsSuggestedActions(string channelId, int buttonCnt = 100)
        {
            switch (channelId)
            {
                case Connector.Channels.Facebook:
                case Connector.Channels.Skype:
                    return buttonCnt <= 10;

                case Connector.Channels.Kik:
                    return buttonCnt <= 20;

                case Connector.Channels.Slack:
                case Connector.Channels.Telegram:
                case Connector.Channels.Emulator:
                case Connector.Channels.Directline:
                case Connector.Channels.Webchat:
                    return buttonCnt <= 100;

                default:
                    return false;
            }
        }

        public static bool SupportsCardActions(string channelId, int buttonCnt = 100)
        {
            switch (channelId)
            {
                case Connector.Channels.Facebook:
                case Connector.Channels.Skype:
                case Connector.Channels.Msteams:
                    return buttonCnt <= 3;

                case Connector.Channels.Slack:
                case Connector.Channels.Emulator:
                case Connector.Channels.Directline:
                case Connector.Channels.Webchat:
                case Connector.Channels.Cortana:
                    return buttonCnt <= 100;

                default:
                    return false;
            }
        }

        public static bool HasMessageFeed(string channelId)
        {
            switch (channelId)
            {
                case Connector.Channels.Cortana:
                    return false;

                default:
                    return true;
            }
        }

        public static int MaxActionTitleLength(string channelId) => 20;

        public static string GetChannelId(ITurnContext turnContext) => string.IsNullOrEmpty(turnContext.Activity.ChannelId)
            ? string.Empty : turnContext.Activity.ChannelId;

        // This class has been deprecated in favor of the class in Microsoft.Bot.Connector.Channels located
        // at https://github.com/Microsoft/botbuilder-dotnet/libraries/Microsoft.Bot.Connector/Channels.cs.
        // This change is non-breaking and this class now inherits from the class in the connector library.
        [Obsolete("This class is deprecated. Please use Microsoft.Bot.Connector.Channels.")]
        public class Channels : Connector.Channels
        {
        }
    }
}
