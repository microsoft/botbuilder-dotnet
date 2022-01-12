// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.OAuth;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Contains settings for an <see cref="OAuthPrompt"/>.
    /// </summary>
    public class OAuthPromptSettings : UserAuthSettings
    {
        /// <summary>
        /// Gets or sets the number of milliseconds the prompt waits for the user to authenticate.
        /// Default is 900,000 (15 minutes).
        /// </summary>
        /// <value>The number of milliseconds the prompt waits for the user to authenticate.</value>
        public int Timeout { get; set; } = (int)TurnStateConstants.OAuthLoginTimeoutValue.TotalMilliseconds;

        /// <summary>
        /// Gets or sets a value indicating whether the auth process should end upon
        /// receiving an invalid message.  Generally the auth process will ignore
        /// incoming messages from the user during the auth flow, if they are not related to the
        /// auth flow.  This flag enables ending the flow rather than ignoring the user's message.
        /// Typically, this flag will be set to 'true', but can be set to 'false' for backwards compatibility.
        /// </summary>
        /// <value>True if the auth flow should automatically end upon receiving an invalid message.</value>
        public bool EndOnInvalidMessage { get; set; } = true;
    }
}
