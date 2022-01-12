// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.OAuth
{
    /// <summary>
    /// Contains settings for User OAuth.
    /// </summary>
    public class UserAuthSettings
    {
        /// <summary>
        /// Gets or sets the <see cref="AppCredentials"/> for <see cref="UserTokenClient"/>.
        /// </summary>
        /// <value>The AppCredentials for OAuthPrompt.</value>
        public AppCredentials OAuthAppCredentials { get; set; }

        /// <summary>
        /// Gets or sets the name of the OAuth connection.
        /// </summary>
        /// <value>The name of the OAuth connection.</value>
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the title of the sign-in or oauth card.
        /// </summary>
        /// <value>The title of the sign-in or oauth card.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets any additional text to include in the sign-in or oauth card.
        /// </summary>
        /// <value>Any additional text to include in the sign-in or oauth card.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets an optional boolean value to force the display of a Sign In link overriding
        /// the default behavior.
        /// </summary>
        /// <value>True to force displaying the SignInLink regardless of channel.</value>
        public bool? ShowSignInLink { get; set; }
    }
}
