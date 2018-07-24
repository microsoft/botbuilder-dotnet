// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// OAuth Related Prompt Settings Which allows a Timeout parameter to be set.
    /// The timeout determines the validity of the token.
    /// </summary>
    public class OAuthPromptSettingsWithTimeout : OAuthPromptSettings
    {
        public int? Timeout { get; set; }
    }
}
