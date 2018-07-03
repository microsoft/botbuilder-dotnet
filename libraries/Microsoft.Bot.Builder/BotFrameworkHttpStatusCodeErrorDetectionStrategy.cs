// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using Microsoft.Rest.TransientFaultHandling;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Bot Framework HTTP Status code error detection strategy.
    /// </summary>
    /// <seealso cref="ITransientErrorDetectionStrategy" />
    public class BotFrameworkHttpStatusCodeErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        /// <summary>
        /// Returns true if status code in HttpRequestExceptionWithStatus exception is RequestTimeout, TooManyRequests, NotFound or greater
        /// than or equal to 500 and not NotImplemented (501) or HttpVersionNotSupported (505).
        /// </summary>
        /// <param name="ex">Exception to check against.</param>
        /// <returns>True if exception is transient otherwise false.</returns>
        public bool IsTransient(Exception ex)
        {
            if (ex != null)
            {
                HttpRequestWithStatusException httpException;
                if ((httpException = ex as HttpRequestWithStatusException) != null)
                {
                    if (httpException.StatusCode == HttpStatusCode.RequestTimeout ||
                        (int)httpException.StatusCode == 429 ||
                        httpException.StatusCode == HttpStatusCode.NotFound ||
                        (httpException.StatusCode >= HttpStatusCode.InternalServerError &&
                            httpException.StatusCode != HttpStatusCode.NotImplemented &&
                            httpException.StatusCode != HttpStatusCode.HttpVersionNotSupported))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
