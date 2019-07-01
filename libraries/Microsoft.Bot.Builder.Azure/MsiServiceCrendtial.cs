using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest;

namespace Microsoft.Bot.Builder.Azure
{
    public class MsiServiceCrendtials : ServiceClientCredentials
    {
        private readonly AzureServiceTokenProvider _tokenProvider;

        public MsiServiceCrendtials(AzureServiceTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string token = await GetTokenAsync().ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private static readonly string OAuthEndpoint = AuthenticationConstants.ToChannelFromBotLoginUrl;
        private static readonly string OAuthScope = AuthenticationConstants.ToChannelFromBotOAuthScope;

        private Task<string> GetTokenAsync()
        {
            return _tokenProvider.GetAccessTokenAsync(OAuthScope);
        }

        public override string ToString()
        {
            return $"msi-{_tokenProvider.PrincipalUsed?.AppId}";
        }
    }
}
