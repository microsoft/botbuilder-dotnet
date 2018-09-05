// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
using System;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Data describing a LUIS _application.
    /// </summary>
    public class LuisApplication
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuisApplication"/> class.
        /// </summary>
        /// <param name="applicationId">LUIS _application ID.</param>
        /// <param name="endpointKey">LUIS subscription or endpoint key.</param>
        /// <param name="endpoint">LUIS endpoint to use.</param>
        public LuisApplication(string applicationId, string endpointKey, string endpoint)
        {
            if (!Guid.TryParse(applicationId, out var appGuid))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid LUIS application id.");
            }

            if (!Guid.TryParse(endpointKey, out var subscriptionGuid))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid LUIS subscription key.");
            }

            if (string.IsNullOrWhiteSpace(endpoint) || !Uri.IsWellFormedUriString(endpoint, UriKind.Absolute))
            {
                throw new ArgumentException($"\"{endpoint}\" is not a valid LUIS endpoint.");
            }

            ApplicationId = applicationId;
            EndpointKey = endpointKey;
            Endpoint = endpoint;
        }

        /// <summary>
        /// Gets or sets LUIS _application ID.
        /// </summary>
        /// <value>
        /// LUIS _application ID.
        /// </value>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets LUIS subscription or endpoint key.
        /// </summary>
        /// <value>
        /// LUIS subscription or endpoint key.
        /// </value>
        public string EndpointKey { get; set; }

        /// <summary>
        /// Gets or sets LUIS endpoint.
        /// </summary>
        /// <value>
        /// LUIS endpoint where application is hosted.
        /// </value>
        public string Endpoint { get; set; }
    }
}
