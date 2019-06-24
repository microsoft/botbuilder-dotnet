// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.StreamingExtensions
{
    public class ReceiveRequest
    {
        /// <summary>
        /// Request verb, null on responses.
        /// </summary>
        public string Verb { get; set; }

        /// <summary>
        /// Request path; null on responses
        /// </summary>
        public string Path { get; set; }

        public List<IContentStream> Streams { get; set; }
    }
}
