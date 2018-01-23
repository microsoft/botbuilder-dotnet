// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public static class HttpClientEx
    {
        /// <summary>
        /// add Bearer authorization token for making API calls
        /// </summary>
        /// <param name="client">The http client</param>
        /// <param name="appId">(default)Setting["microsoftAppId"]</param>
        /// <param name="password">(default)Setting["microsoftAppPassword"]</param>
        /// <returns>HttpClient with Bearer Authorization header</returns>
        public static async Task AddAPIAuthorization(this HttpClient client, string appId = null, string password = null)
        {
            var token = await new MicrosoftAppCredentials(appId, password).GetTokenAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
