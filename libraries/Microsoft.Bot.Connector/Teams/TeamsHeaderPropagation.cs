// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        public static HeaderPropagationEntryCollection GetHeadersToPropagate()
        {
            // Propagate headers to the outgoing request by adding them to the HeaderPropagationEntryCollection. For example:
            var headersToPropagate = new HeaderPropagationEntryCollection();

            headersToPropagate.Propagate("X-Ms-Teams-Id");
            headersToPropagate.Add("X-Ms-Teams-Custom", new StringValues("Custom-Value"));
            headersToPropagate.Append("X-Ms-Teams-Channel", new StringValues("-SubChannel-Id"));
            headersToPropagate.Override("X-Ms-Other", new StringValues("new-value"));

            return headersToPropagate;
        }
    }
}
