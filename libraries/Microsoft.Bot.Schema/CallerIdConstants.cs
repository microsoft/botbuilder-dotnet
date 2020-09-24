// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Constants used to populate the <see cref="Activity.CallerId"/> property.
    /// </summary>
    public static class CallerIdConstants
    {
        /// <summary>
        ///  The caller ID for any Bot Framework channel.
        /// </summary>
        public const string PublicAzureChannel = "urn:botframework:azure";

        /// <summary>
        ///  The caller ID for any Bot Framework US Government cloud channel.
        /// </summary>
        public const string USGovChannel = "urn:botframework:azureusgov";

        /// <summary>
        /// The caller ID prefix when a bot initiates a request to another bot.
        /// </summary>
        /// <remarks>
        /// This prefix will be followed by the Azure Active Directory App ID of the bot that initiated the call.
        /// </remarks>
        public const string BotToBotPrefix = "urn:botframework:aadappid:";
    }
}
