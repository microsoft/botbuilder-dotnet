// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    /// <summary>
    /// Options class for Facebook Adapter.
    /// </summary>
    public class FacebookClientWrapperOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookClientWrapperOptions"/> class.
        /// </summary>
        /// <param name="verifyToken">The token used to validate that incoming webhooks are originated from Facebook.</param>
        /// <param name="appSecret">The app secret.</param>
        /// <param name="accessToken">The Facebook access token.</param>
        /// <param name="apiHost">A token for validating the origin of incoming webhooks.</param>
        /// <param name="apiVersion">A token for a bot to work on a single workspace.</param>
        public FacebookClientWrapperOptions(string verifyToken, string appSecret, string accessToken, string apiHost = "graph.facebook.com", string apiVersion = "v3.2")
        {
            FacebookVerifyToken = verifyToken;
            FacebookAppSecret = appSecret;
            FacebookAccessToken = accessToken;
            FacebookApiHost = apiHost;
            FacebookApiVersion = apiVersion;
        }

        /// <summary>
        /// Gets or sets the alternate root URL used to construct calls to Facebook's API. Defaults to "graph.facebook.com" but can be changed (for mocking, proxy, etc).
        /// </summary>
        /// <value>The API host.</value>
        public string FacebookApiHost { get; set; }

        /// <summary>
        /// Gets or sets the alternate API version used to construct calls to Facebook's API. Defaults to "v3.2".
        /// </summary>
        /// <value>The API version.</value>
        public string FacebookApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the verify token used to initially create and verify the webhooks subscription settings on Facebook's developer portal.
        /// </summary>
        /// <value>The verify token.</value>
        public string FacebookVerifyToken { get; set; }

        /// <summary>
        /// Gets or sets the app secret from the **Basic Settings** page from your app's configuration in the Facebook developer portal.
        /// </summary>
        /// <value>The app secret.</value>
        public string FacebookAppSecret { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// When bound to a single page, use `access_token` to specify the "page access token" provided in the Facebook developer portal's "Access Tokens" widget of the "Messenger Settings" page.
        /// </summary>
        /// <value>The access token.</value>
        public string FacebookAccessToken { get; set; }
    }
}
