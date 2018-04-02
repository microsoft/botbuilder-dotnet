// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Authentication helpers.
    /// </summary>
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Call context storage to propagate values throught the request.
        /// </summary>
        private static AsyncLocal<BotFrameworkAuthenticationContext> asyncLocal = new AsyncLocal<BotFrameworkAuthenticationContext>();

        /// <summary>
        /// Sets the request authentication context.
        /// </summary>
        public static void SetRequestAuthenticationContext(BotFrameworkAuthenticationContext authenticationContext)
        {
            asyncLocal.Value = authenticationContext;
        }

        /// <summary>
        /// Gets the request context.
        /// </summary>
        /// <returns>Request context.</returns>
        public static BotFrameworkAuthenticationContext GetBotFrameworkAuthenticationContext()
        {
            try
            {
                return asyncLocal.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
