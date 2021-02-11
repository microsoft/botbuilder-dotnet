// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Extension methods for BotSignIn.
    /// </summary>
    public static partial class BotSignInExtensions
    {
        /// <summary>Get sign-in URL.</summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='state'>State.</param>
        /// <param name='codeChallenge'>Code challenge.</param>
        /// <param name='emulatorUrl'>Emulator URL.</param>
        /// <param name='finalRedirect'>Final redirect.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>The sign-in URL.</returns>
        public static async Task<string> GetSignInUrlAsync(this IBotSignIn operations, string state, string codeChallenge = default(string), string emulatorUrl = default(string), string finalRedirect = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var result = await operations.GetSignInUrlWithHttpMessagesAsync(state, codeChallenge, emulatorUrl, finalRedirect, null, cancellationToken).ConfigureAwait(false))
                {
                    return result.Body;
                }
            }

        /// <summary>Get sign-in resource with HTTP message.</summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='state'>State.</param>
        /// <param name='codeChallenge'>Code challenge.</param>
        /// <param name='emulatorUrl'>Emulator URL.</param>
        /// <param name='finalRedirect'>Final redirect.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>The <see cref="SignInResource"/>.</returns>
        public static async Task<SignInResource> GetSignInResourceAsync(this OAuthClient operations, string state, string codeChallenge = default(string), string emulatorUrl = default(string), string finalRedirect = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var result = await operations.GetSignInResourceWithHttpMessagesAsync(state, codeChallenge, emulatorUrl, finalRedirect, null, cancellationToken).ConfigureAwait(false))
                {
                    return result.Body;
                }
            }
    }
}
