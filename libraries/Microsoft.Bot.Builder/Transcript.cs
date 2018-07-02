// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Transcript store item.
    /// </summary>
    public class Transcript
    {
        /// <summary>
        /// ChannelId that the transcript was taken from.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Conversation Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Date conversation was started.
        /// </summary>
        public DateTimeOffset Created { get; set; }
    }
}
