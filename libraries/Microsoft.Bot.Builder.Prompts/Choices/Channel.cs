// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public class Channel
    {
        public class Channels
        {
            public const string Facebook = "facebook";
            public const string Skype = "skype";
            public const string Msteams = "msteams";
            public const string Telegram = "telegram";
            public const string Kik = "kik";
            public const string Email = "email";
            public const string Slack = "slack";
            public const string Groupme = "groupme";
            public const string Sms = "sms";
            public const string Emulator = "emulator";
            public const string Directline = "directline";
            public const string Webchat = "webchat";
            public const string Console = "console";
            public const string Cortana = "cortana";
        };

        public static bool SupportsSuggestedActions(string channelId, int buttonCnt = 100)
        {
            switch (channelId)
            {
                case Channels.Facebook:
                case Channels.Skype:
                    return (buttonCnt <= 10);

                case Channels.Kik:
                    return (buttonCnt <= 20);

                case Channels.Slack:
                case Channels.Telegram:
                case Channels.Emulator:
                    return (buttonCnt <= 100);

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
                    return (buttonCnt <= 3);

                case Channels.Slack:
                case Channels.Emulator:
                case Channels.Directline:
                case Channels.Webchat:
                case Channels.Cortana:
                    return (buttonCnt <= 100);

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

        public static int MaxActionTitleLength(string channelId)
        {
            return 20;
        }

        public static string GetChannelId(ITurnContext context)
        {
            return string.IsNullOrEmpty(context.Activity.ChannelId) ? string.Empty : context.Activity.ChannelId;
        }
    }
}