// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.StreamingExtensions
{
    public class ReceiveResponse
    {
        /// <summary>
        /// Status - The Response Status
        /// </summary>
        public int StatusCode { get; set; }

        public List<IContentStream> Streams { get; set; }
    }
}
