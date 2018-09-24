// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A collection of Azure Active Directory resource URLs
    /// </summary>
    public class AadResourceUrls
    {
        /// <summary>
        /// An array of resource URLs to use with Azure Active Directory
        /// </summary>
        [JsonProperty("resourceUrls")]
        public string[] ResourceUrls { get; set; }
    }
}
