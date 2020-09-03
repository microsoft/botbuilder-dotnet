// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Represents a Cloud Environment used to authenticate Bot Framework Protocol network calls within this environment.
    /// </summary>
    public interface ICloudEnvironment
    {
        /// <summary>
        /// Validate Bot Framework Protocol requests.
        /// </summary>
        /// <param name="activity">The inbound Activity.</param>
        /// <param name="authHeader">The http auth header.</param>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="authConfiguration">The AuthenticationConfiguration used for Skills.</param>
        /// <param name="httpClient">The httpClient to use for making calls to teh auth service.</param>
        /// <param name="logger">A ILogger to use while performing authentication.</param>
        /// <returns>Asynchronous Task with Authentication results.</returns>
        Task<(ClaimsIdentity claimsIdentity, ServiceClientCredentials credentials, string scope, string callerId)> AuthenticateRequestAsync(Activity activity, string authHeader, ICredentialProvider credentialProvider, AuthenticationConfiguration authConfiguration, HttpClient httpClient, ILogger logger);
    }
}
