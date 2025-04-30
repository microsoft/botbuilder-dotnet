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
        private static readonly AsyncLocal<IDictionary<string, StringValues>> _headers = new ();

        private static readonly AsyncLocal<IDictionary<string, StringValues>> _headersToPropagate = new ();

        /// <summary>
        /// Gets or sets the headers from an incoming request.
        /// </summary>
        /// <value>.</value>
        public static IDictionary<string, StringValues> Headers
        {
            get => _headers.Value ?? new Dictionary<string, StringValues>();
            set => _headers.Value = value;
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
        /// Filters the headers to only include those that are relevant for propagation.
        /// </summary>
        /// <returns>The filtered headers.</returns>
        public static IDictionary<string, StringValues> FilterHeaders()
        {
            var filteredHeaders = new Dictionary<string, StringValues>();

            if (Headers.TryGetValue("X-Ms-Correlation-Id", out var value))
            {
                filteredHeaders.Add("X-Ms-Correlation-Id", value);
            }

            foreach (var header in HeadersToPropagate)
            {
                if (!string.IsNullOrEmpty(header.Value))
                {
                    filteredHeaders.Add(header.Key, header.Value);
                }
                else
                {
                    if (Headers.TryGetValue(header.Key, out var headerValue))
                    {
                        filteredHeaders.Add(header.Key, headerValue);
                    }
                }
            }

            return filteredHeaders;
        }
    }
}
