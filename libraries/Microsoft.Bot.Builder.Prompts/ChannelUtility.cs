using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Prompts
{
    public static class ChannelUtility
    {
        private const string facebook = "facebook";
        private const string skype = "skype";
        private const string msteams = "msteams";
        private const string telegram = "telegram";
        private const string kik = "kik";
        private const string email = "email";
        private const string slack = "slack";
        private const string groupme = "groupme";
        private const string sms = "sms";
        private const string emulator = "emulator";
        private const string directline = "directline";
        private const string webchat = "webchat";
        private const string console = "console";
        private const string cortana = "cortana";

        public static string GetChannelId(ITurnContext context) => context?.Activity?.ChannelId ?? "";

        public static bool SupportsSuggestedActions(string channelId, int buttonCnt = 100)
        {
            switch (channelId)
            {
                case facebook:
                case skype:
                    return (buttonCnt <= 10);
                case kik:
                    return (buttonCnt <= 20);
                case slack:
                case telegram:
                case emulator:
                    return (buttonCnt <= 100);
                default:
                    return false;
            }
        }

        public static bool SupportsCardActions(string channelId, int buttonCnt = 100)
        {
            switch (channelId)
            {
                case facebook:
                case skype:
                case msteams:
                    return (buttonCnt <= 3);
                case slack:
                case emulator:
                case directline:
                case webchat:
                case cortana:
                    return (buttonCnt <= 100);
                default:
                    return false;
            }
        }

        public static bool HasMessageFeed(string channelId)
        {

            switch (channelId)
            {
                case cortana:
                    return false;
                default:
                    return true;
            }
        }

        public static int MaxActionTitleLength(string channelId)
        {
            return 20;
        }
    }
}
