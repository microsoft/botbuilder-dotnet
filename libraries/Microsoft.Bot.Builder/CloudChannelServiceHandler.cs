// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A class to help with the implementation of the Bot Framework protocol using BotFrameworkAuthentication.
    /// </summary>
    public class CloudChannelServiceHandler : ChannelServiceHandlerBase
    {
        private readonly BotFrameworkAuthentication _auth;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudChannelServiceHandler"/> class, using Bot Framework Authentication.
        /// </summary>
        /// <param name="auth">The Bot Framework Authentication object.</param>
        public CloudChannelServiceHandler(BotFrameworkAuthentication auth)
        {
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }

        /// <inheritdoc/>
        internal override async Task<ClaimsIdentity> AuthenticateAsync(string authHeader, CancellationToken cancellationToken)
        {
            return await _auth.AuthenticateChannelRequestAsync(authHeader, cancellationToken).ConfigureAwait(false);
        }
    }
}
