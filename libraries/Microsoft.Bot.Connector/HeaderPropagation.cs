// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Class to handle header propagation from incoming request to outgoing request.
    /// </summary>
    public static class HeaderPropagation
    {
        private static readonly AsyncLocal<IDictionary<string, StringValues>> _requestHeaders = new ();

        private static readonly AsyncLocal<IDictionary<string, StringValues>> _headersToPropagate = new ();

        /// <summary>
        /// Gets or sets the headers from an incoming request.
        /// </summary>
        /// <value>.</value>
        public static IDictionary<string, StringValues> RequestHeaders
        {
            get => _requestHeaders.Value ?? new Dictionary<string, StringValues>();
            set => _requestHeaders.Value = value;
        }

        /// <summary>
        /// Gets or sets the selected headers for propagation.
        /// </summary>
        /// <value>.</value>
        public static IDictionary<string, StringValues> HeadersToPropagate
        {
            get => _headersToPropagate.Value ?? new Dictionary<string, StringValues>();
            set => _headersToPropagate.Value = value;
        }

        /// <summary>
        /// Filters the request's headers to only include those that are relevant for propagation.
        /// </summary>
        /// <param name="headerFilter">The headers to filter.</param>
        /// <returns>The filtered headers.</returns>
        public static IDictionary<string, StringValues> FilterHeaders(HeaderPropagationEntryCollection headerFilter)
        {
            // We propagate the X-Ms-Correlation-Id header by default.
            headerFilter.Propagate("X-Ms-Correlation-Id");
         
            var filteredHeaders = new Dictionary<string, StringValues>();

            foreach (var filter in headerFilter.Entries)
            {
                if (RequestHeaders.TryGetValue(filter.Key, out var value))
                {
                    switch (filter.Action)
                    {
                        case HeaderPropagationEntryAction.Add:
                            break;
                        case HeaderPropagationEntryAction.Append:
                            filteredHeaders.Add(filter.Key, string.Concat(value, filter.Value));
                            break;
                        case HeaderPropagationEntryAction.Override:
                            filteredHeaders.Add(filter.Key, filter.Value);
                            break;
                        case HeaderPropagationEntryAction.Propagate:
                            filteredHeaders.Add(filter.Key, value);
                            break;
                    }
                }
                else
                {
                    switch (filter.Action)
                    {
                        case HeaderPropagationEntryAction.Add:
                            filteredHeaders.Add(filter.Key, filter.Value);
                            break;
                        case HeaderPropagationEntryAction.Override:
                            filteredHeaders.Add(filter.Key, filter.Value);
                            break;
                    }
                }
            }

            return filteredHeaders;
        }
    }
}
