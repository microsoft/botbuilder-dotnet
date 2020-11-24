﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using Microsoft.Bot.Schema;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for BotSignIn.
    /// </summary>
    public static partial class BotSignInExtensions
    {
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='state'>
            /// </param>
            /// <param name='codeChallenge'>
            /// </param>
            /// <param name='emulatorUrl'>
            /// </param>
            /// <param name='finalRedirect'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<string> GetSignInUrlAsync(this IBotSignIn operations, string state, string codeChallenge = default(string), string emulatorUrl = default(string), string finalRedirect = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetSignInUrlWithHttpMessagesAsync(state, codeChallenge, emulatorUrl, finalRedirect, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='state'>
            /// </param>
            /// <param name='codeChallenge'>
            /// </param>
            /// <param name='emulatorUrl'>
            /// </param>
            /// <param name='finalRedirect'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<SignInResource> GetSignInResourceAsync(this OAuthClient operations, string state, string codeChallenge = default(string), string emulatorUrl = default(string), string finalRedirect = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetSignInResourceWithHttpMessagesAsync(state, codeChallenge, emulatorUrl, finalRedirect, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}
