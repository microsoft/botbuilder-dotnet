// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    internal sealed class Authenticators : IAuthenticator
    {
        private readonly IAuthenticator[] inners;

        public Authenticators(params IAuthenticator[] inners)
        {
            this.inners = inners ?? throw new ArgumentNullException(nameof(inners));
        }

        async Task<AuthenticatorResult> IAuthenticator.GetTokenAsync(bool forceRefresh)
        {
            ExceptionDispatchInfo info = null;

            for (int index = 0; index < this.inners.Length; ++index)
            {
                var inner = this.inners[index];
                try
                {
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
