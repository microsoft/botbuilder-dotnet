// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Bot.Configuration;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Data describing a LUIS application.
    /// </summary>
    public class LuisApplication
    {
        public LuisApplication()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisApplication"/> class.
        /// </summary>
        /// <param name="applicationId">LUIS application ID.</param>
        /// <param name="endpointKey">LUIS subscription or endpoint key.</param>
        /// <param name="endpoint">LUIS endpoint to use like https://westus.api.cognitive.microsoft.com.</param>
        public LuisApplication(string applicationId, string endpointKey, string endpoint)
        {
            if (!Guid.TryParse(applicationId, out var appGuid))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid LUIS application id.");
            }

            if (!Guid.TryParse(endpointKey, out var subscriptionGuid))
            {
                throw new ArgumentException($"\"{subscriptionGuid}\" is not a valid LUIS subscription key.");
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException($"\"{endpoint}\" is not a valid LUIS endpoint.");
            }

            if (!Uri.IsWellFormedUriString(endpoint, UriKind.Absolute))
            {
                throw new ArgumentException($"\"{endpoint}\" is not a valid LUIS endpoint.");
            }

            ApplicationId = applicationId;
            EndpointKey = endpointKey;
            Endpoint = endpoint;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisApplication"/> class.
        /// </summary>
        /// <param name="service">LUIS coonfiguration.</param>
        public LuisApplication(LuisService service)
            : this(service.AppId, service.SubscriptionKey, service.GetEndpoint())
        {
        }

        /// <summary>
        /// Gets or sets lUIS application ID.
        /// </summary>
        /// <value>
        /// LUIS application ID.
        /// </value>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets lUIS subscription or endpoint key.
        /// </summary>
        /// <value>
        /// LUIS subscription or endpoint key.
        /// </value>
        public string EndpointKey { get; set; }

        /// <summary>
        /// Gets or sets lUIS endpoint like https://westus.api.cognitive.microsoft.com.
        /// </summary>
        /// <value>
        /// LUIS endpoint where application is hosted.
        /// </value>
        public string Endpoint { get; set; }
    }
}
