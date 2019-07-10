using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Credential provider that uses managed service identity instead of client secrets.
    /// </summary>
    public class MsiCredentialProvider : ICredentialProvider2
    {
        private readonly IConfiguration _configuration;
        private readonly AzureServiceTokenProvider _tokenProvider;
        public MsiCredentialProvider(IConfiguration configuration, AzureServiceTokenProvider tokenProvider)
        {
            _configuration = configuration;
            _tokenProvider = tokenProvider;
        }

        public Task<string> GetAppPasswordAsync(string appId)
        {
            return Task.FromResult(String.Empty);
        }

        public Task<bool> IsAuthenticationDisabledAsync()
        {
            return Task.FromResult(string.IsNullOrEmpty(_configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value));
        }

        public Task<bool> IsValidAppIdAsync(string appId)
        {
            return Task.FromResult(true);
        }

        public ServiceClientCredentials GetCredentials()
        {
            return new MsiServiceCredentials(_tokenProvider);
        }
    }
}
