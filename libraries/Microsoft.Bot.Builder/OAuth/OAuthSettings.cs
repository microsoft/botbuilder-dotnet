// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Contains settings for User OAuth.
    /// </summary>
    public class OAuthSettings
    {
        /// <summary>
        /// Gets or sets the OAuthAppCredentials for OAuthPrompt.
        /// </summary>
        /// <value>The AppCredentials for OAuthPrompt.</value>
        public AppCredentials OAuthAppCredentials { get; set; }

        /// <summary>
        /// Gets or sets the name of the OAuth connection.
        /// </summary>
        /// <value>The name of the OAuth connection.</value>
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the title of the sign-in card.
        /// </summary>
        /// <value>The title of the sign-in card.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets any additional text to include in the sign-in card.
        /// </summary>
        /// <value>Any additional text to include in the sign-in card.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds the prompt waits for the user to authenticate.
        /// Default is 900,000 (15 minutes).
        /// </summary>
        /// <value>The number of milliseconds the prompt waits for the user to authenticate.</value>
        public int? Timeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the auth process should end upon
        /// receiving an invalid message.  Generally the auth process will ignore
        /// incoming messages from the user during the auth flow, if they are not related to the
        /// auth flow.  This flag enables ending the flow rather than ignoring the user's message.
        /// Typically, this flag will be set to 'true', but can be set to 'false' for backwards compatibility.
        /// </summary>
        /// <value>True if the auth flow should automatically end upon receiving an invalid message.</value>
        public bool EndOnInvalidMessage { get; set; } = true;

        /// <summary>
        /// Gets or sets an optional boolean value to force the display of a Sign In link overriding
        /// the default behavior.
        /// </summary>
        /// <value>True to display the SignInLink.</value>
        public bool? ShowSignInLink { get; set; }
    }
}
