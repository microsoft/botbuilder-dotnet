// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Web;
using Microsoft.Bot.Configuration;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Data describing a LUIS application.
    /// </summary>
    public class LuisApplication
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuisApplication"/> class.
        /// </summary>
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
            : this((applicationId, endpointKey, endpoint))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisApplication"/> class.
        /// </summary>
        /// <param name="service">LUIS configuration.</param>
        public LuisApplication(LuisService service)
            : this((service.AppId, service.SubscriptionKey, service.GetEndpoint()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisApplication"/> class.
        /// </summary>
        /// <param name="applicationEndpoint">LUIS application endpoint.</param>
        public LuisApplication(string applicationEndpoint)
            : this(Parse(applicationEndpoint))
        {
        }

        private LuisApplication(ValueTuple<string, string, string> props)
        {
            var (applicationId, endpointKey, endpoint) = props;

            if (!Guid.TryParse(applicationId, out var _))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid LUIS application id.");
            }

            if (!Guid.TryParse(endpointKey, out var _))
            {
                throw new ArgumentException($"\"{endpointKey}\" is not a valid LUIS subscription key.");
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                endpoint = "https://westus.api.cognitive.microsoft.com";
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

        private static (string applicationId, string endpointKey, string endpoint) Parse(string applicationEndpoint)
        {
            if (!Uri.TryCreate(applicationEndpoint, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException($"Unable to create the LUIS endpoint with the given {applicationEndpoint}.", nameof(applicationEndpoint));
            }

            var applicationId = string.Empty;

            var segments = uri.Segments;
            for (var segment = 0; segment < segments.Length - 1; segment++)
            {
                if (segments[segment] == "apps/")
                {
                    applicationId = segments[segment + 1].TrimEnd('/');
                    break;
                }
            }

            if (string.IsNullOrEmpty(applicationId))
            {
                throw new ArgumentException($"Could not find application Id in {applicationEndpoint}");
            }

            var endpointKey = HttpUtility.ParseQueryString(uri.Query).Get("subscription-key");
            var endpoint = uri.GetLeftPart(UriPartial.Authority);
            return (applicationId, endpointKey, endpoint);
        }
    }
}
