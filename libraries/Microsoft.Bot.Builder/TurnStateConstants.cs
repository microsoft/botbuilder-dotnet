// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Constants used in TurnState.
    /// </summary>
    public static class TurnStateConstants
    {
        /// <summary>
        /// TurnState key for the OAuth login timeout.
        /// </summary>
        public const string OAuthLoginTimeoutKey = "loginTimeout";

        /// <summary>
        /// Name of the token polling settings key.
        /// </summary>
        public const string TokenPollingSettingsKey = "tokenPollingSettings";

        /// <summary>
        /// Default amount of time an OAuthCard will remain active (clickable and actively waiting for a token).
        /// After this time:
        /// (1) the OAuthCard will not allow the user to click on it.
        /// (2) any polling triggered by the OAuthCard will stop.
        /// </summary>
        public static readonly TimeSpan OAuthLoginTimeoutValue = TimeSpan.FromMinutes(15);
    }
}
