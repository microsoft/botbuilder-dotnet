// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Methods for determining channel specific functionality.
    /// </summary>
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
    public class Channel
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        /// <summary>
        /// Determine if a number of Suggested Actions are supported by a Channel.
        /// </summary>
        /// <param name="channelId">The Channel to check the if Suggested Actions are supported in.</param>
        /// <param name="buttonCnt">(Optional) The number of Suggested Actions to check for the Channel.</param>
        /// <returns>True if the Channel supports the buttonCnt total Suggested Actions, False if the Channel does not support that number of Suggested Actions.</returns>
        public static bool SupportsSuggestedActions(string channelId, int buttonCnt = 100)
        {
            switch (channelId)
            {
                // https://developers.facebook.com/docs/messenger-platform/send-messages/quick-replies
                case Connector.Channels.Facebook:
                case Connector.Channels.Skype:
                    return buttonCnt <= 10;

                // https://developers.line.biz/en/reference/messaging-api/#items-object
                case Connector.Channels.Line:
                    return buttonCnt <= 13;

                // https://dev.kik.com/#/docs/messaging#text-response-object
                case Connector.Channels.Kik:
                    return buttonCnt <= 20;

                case Connector.Channels.Telegram:
                case Connector.Channels.Emulator:
                case Connector.Channels.Directline:
                case Connector.Channels.DirectlineSpeech:
                case Connector.Channels.Webchat:
                    return buttonCnt <= 100;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determine if a number of Card Actions are supported by a Channel.
        /// </summary>
        /// <param name="channelId">The Channel to check if the Card Actions are supported in.</param>
        /// <param name="buttonCnt">(Optional) The number of Card Actions to check for the Channel.</param>
        /// <returns>True if the Channel supports the buttonCnt total Card Actions, False if the Channel does not support that number of Card Actions.</returns>
        public static bool SupportsCardActions(string channelId, int buttonCnt = 100)
        {
            switch (channelId)
            {
                case Connector.Channels.Facebook:
                case Connector.Channels.Skype:
                case Connector.Channels.Msteams:
                    return buttonCnt <= 3;

                case Connector.Channels.Line:
                    return buttonCnt <= 99;

                case Connector.Channels.Slack:
                case Connector.Channels.Emulator:
                case Connector.Channels.Directline:
                case Connector.Channels.DirectlineSpeech:
                case Connector.Channels.Webchat:
                case Connector.Channels.Cortana:
                    return buttonCnt <= 100;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determine if a Channel has a Message Feed.
        /// </summary>
        /// <param name="channelId">The Channel to check for Message Feed.</param>
        /// <returns>True if the Channel has a Message Feed, False if it does not.</returns>
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

        /// <summary>
        /// Maximum length allowed for Action Titles.
        /// </summary>
        /// <param name="channelId">The Channel to determine Maximum Action Title Length.</param>
        /// <returns>The total number of characters allowed for an Action Title on a specific Channel.</returns>
#pragma warning disable CA1801 // Review unused parameters (we can't remove the channelId parameter without breaking binary compatibility)
        public static int MaxActionTitleLength(string channelId) => 20;
#pragma warning restore CA1801 // Review unused parameters

        /// <summary>
        /// Get the Channel Id from the current Activity on the Turn Context.
        /// </summary>
        /// <param name="turnContext">The Turn Context to retrieve the Activity's Channel Id from.</param>
        /// <returns>The Channel Id from the Turn Context's Activity.</returns>
        public static string GetChannelId(ITurnContext turnContext) => string.IsNullOrEmpty(turnContext.Activity.ChannelId)
            ? string.Empty
            : turnContext.Activity.ChannelId;

        // This class has been deprecated in favor of the class in Microsoft.Bot.Connector.Channels located
        // at https://github.com/Microsoft/botbuilder-dotnet/libraries/Microsoft.Bot.Connector/Channels.cs.
        // This change is non-breaking and this class now inherits from the class in the connector library.
        [Obsolete("This class is deprecated. Please use Microsoft.Bot.Connector.Channels.")]
#pragma warning disable CA1034 // Nested types should not be visible (this type is deprecated, ignoring)
#pragma warning disable CA1724 // Namespace name conflict (this type is deprecated, ignoring)
        public class Channels : Connector.Channels
#pragma warning restore CA1724 // Namespace name conflict
#pragma warning restore CA1034 // Nested types should not be visible
        {
        }
    }
}
