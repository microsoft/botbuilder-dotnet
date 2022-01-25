// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    /// <summary>A Facebook read message, including watermark of messages that were read.</summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class FacebookRead
    {
        /// <summary>
        /// Gets or sets the timestamp were messages were read.
        /// </summary>
        /// <value>
        /// All messages that were sent before or at this timestamp were read.
        /// </value>
        [JsonProperty("watermark")]
        public long Watermark { get; set; }
    }
}
