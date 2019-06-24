using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Bot.Connector.Authentication
{
    internal class ConstantHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient httpClient;

        public ConstantHttpClientFactory(HttpClient client)
        {
            httpClient = client ?? throw new ArgumentNullException(nameof(client));
        }

        public HttpClient GetHttpClient()
        {
            return httpClient;
        }
    }
}
