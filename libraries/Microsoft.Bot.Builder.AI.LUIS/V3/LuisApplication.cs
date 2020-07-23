// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Web;
using LuisV2 = Microsoft.Bot.Builder.AI.Luis;

namespace Microsoft.Bot.Builder.AI.LuisV3
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

            if (!Guid.TryParse(endpointKey, out var subscriptionGuid))
            {
                throw new ArgumentException($"\"{subscriptionGuid}\" is not a valid LUIS subscription key.");
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

            string applicationId = null;
            var foundApps = false;
            foreach (var segment in uri.Segments)
            {
                if (foundApps)
                {
                    applicationId = segment.TrimEnd('/');
                    break;
                }

                if (segment == "apps/")
                {
                    foundApps = true;
                }
            }

            if (applicationId == null)
            {
                throw new ArgumentException($"Could not find application Id in {applicationEndpoint}");
            }

            var endpointKey = HttpUtility.ParseQueryString(uri.Query).Get("subscription-key");
            var endpoint = uri.GetLeftPart(UriPartial.Authority);
            return (applicationId, endpointKey, endpoint);
        }
    }
}
