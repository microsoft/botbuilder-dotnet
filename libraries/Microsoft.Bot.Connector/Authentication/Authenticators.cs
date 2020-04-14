// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Apply Delegation Pattern to services that implement IAuthenticator.
    /// (https://en.wikipedia.org/wiki/Delegation_pattern).
    /// This service composes multiple authenticators into a single authenticator.
    /// </summary>
    internal sealed class Authenticators : IAuthenticator
    {
        private readonly IAuthenticator[] _inners;

        public Authenticators(params IAuthenticator[] inners)
        {
            _inners = inners ?? throw new ArgumentNullException(nameof(inners));
        }

        async Task<AuthenticatorResult> IAuthenticator.GetTokenAsync(bool forceRefresh)
        {
            ExceptionDispatchInfo info = null;

            // iterate over the inner IAuthenticator services
            foreach (var inner in _inners)
            {
                try
                {
                    // delegate the method invocation to the inner IAuthenticator service
                    return await inner.GetTokenAsync(forceRefresh).ConfigureAwait(false);
                }
                catch (Exception error)
                {
                    // always keep the last failure
                    info = ExceptionDispatchInfo.Capture(error);
                }
            }

            // if there were any failures, rethrow and preserve the stack trace
            info?.Throw();

            // we should not get here unless there were zero inner IAuthenticator instances
            throw new InvalidOperationException();
        }
    }
}
