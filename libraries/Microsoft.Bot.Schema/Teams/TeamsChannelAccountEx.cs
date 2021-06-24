// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Teams Channel Account extensions.
    /// </summary>
    public partial class TeamsChannelAccount
    {
        /// <summary>
        /// Gets or sets the AAD Object Id.
        /// </summary>
        [JsonProperty(PropertyName = "objectId")]
        private string ObjectId
        {
            get => AadObjectId;

            set => AadObjectId = value;
        }
    }
}
