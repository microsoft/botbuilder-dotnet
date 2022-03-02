// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Invoke request body type for app-based link query.
    /// </summary>
    public class AppBasedLinkQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppBasedLinkQuery"/> class.
        /// </summary>
        public AppBasedLinkQuery()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the  <see cref="AppBasedLinkQuery"/> class.
        /// </summary>
        /// <param name="url">Url queried by user.</param>
        public AppBasedLinkQuery(string url = default)
        {
            Url = url;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets url queried by user.
        /// </summary>
        /// <value>The URL queried by user.</value>
        [JsonPropertyName("url")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string Url { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets state, which is the magic code for OAuth Flow.
        /// </summary>
        /// <value>The state, which is the magic code for OAuth Flow.</value>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
