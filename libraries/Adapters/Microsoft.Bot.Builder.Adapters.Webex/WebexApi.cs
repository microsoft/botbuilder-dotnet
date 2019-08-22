// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Thrzn41.WebexTeams;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    public class WebexApi : IWebexClient
    {
        private TeamsAPIClient _api;

        /// <summary>
        /// Creates a Webex client by supplying the access token.
        /// </summary>
        /// <param name="accessToken">The access token of the Webex account.</param>
        public void CreateClient(string accessToken)
        {
            _api = TeamsAPI.CreateVersion1Client(accessToken);
        }

        public async Task<string> CreateMessageAsync(string toPersonOrEmail, string text)
        {
            var webexResponse = await _api.CreateDirectMessageAsync(toPersonOrEmail, text).ConfigureAwait(false);
            return webexResponse.Data.Id;
        }
    }
}
