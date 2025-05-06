// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Bot.Connector.Teams
{
    /// <summary>
    /// Instantiate this class to set the headers to propagate from incoming request to outgoing request.
    /// </summary>
    public static class TeamsHeaderPropagation
    {
        /// <summary>
        /// Returns the headers to propagate from incoming request to outgoing request.
        /// </summary>
        /// <returns>.</returns>
        public static Dictionary<string, StringValues> GetHeadersToPropagate()
        {
            var headersToPropagate = new Dictionary<string, StringValues>
            {
                ["X-Ms-Teams-Id"] = string.Empty,
                ["X-Ms-Teams-Custom"] = "Custom-Value"
            };

            return headersToPropagate;
            
            // HeaderPropagation.HeadersToPropagate = headersToPropagate;
        }
    }
}
