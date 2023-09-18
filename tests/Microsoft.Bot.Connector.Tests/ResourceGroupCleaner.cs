// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Tests
{
    public class ResourceGroupCleaner : DelegatingHandler
    {
        private Regex _resourceGroupPattern = new Regex(@"/subscriptions/[^/]+/resourcegroups/([^?]+)\?api-version");
        private HashSet<string> _resourceGroupsCreated = new HashSet<string>();
        private TokenCredentials _tokenCredentials;

        public ResourceGroupCleaner(TokenCredentials tokenCredentials)
        {
            _tokenCredentials = tokenCredentials;
        }

        public async Task DeleteResourceGroups()
        {
            HttpClient httpClient = new HttpClient();
            foreach (var resourceGroupUri in _resourceGroupsCreated)
            {
                HttpRequestMessage httpRequest = new HttpRequestMessage();
                httpRequest.Method = new HttpMethod("DELETE");
                httpRequest.RequestUri = new Uri(resourceGroupUri);

                _tokenCredentials.ProcessHttpRequestAsync(httpRequest, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

                HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest).ConfigureAwait(false);
                string groupName = _resourceGroupPattern.Match(resourceGroupUri).Groups[1].Value;
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Started deletion of resource group '{0}'. Server responded with status code {1}.",
                    groupName,
                    httpResponse.StatusCode);
                Console.WriteLine(message);
                Debug.WriteLine(message);
            }
        }    

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_resourceGroupPattern.IsMatch(request.RequestUri.AbsoluteUri) &&
                request.Method == HttpMethod.Put)
            {
                _resourceGroupsCreated.Add(request.RequestUri.AbsoluteUri);
            }

            return base.SendAsync(request, cancellationToken);
        }        
    }
}
