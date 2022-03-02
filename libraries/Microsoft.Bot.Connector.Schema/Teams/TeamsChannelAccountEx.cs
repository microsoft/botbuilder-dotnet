// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Teams Channel Account extensions.
    /// </summary>
    public partial class TeamsChannelAccount
    {
        /// <summary>
        /// Gets or sets the AAD Object Id.
        /// </summary>
        [JsonPropertyName("objectId")]
        private string ObjectId
        {
            get => AadObjectId;

            set => AadObjectId = value;
        }
    }
}
