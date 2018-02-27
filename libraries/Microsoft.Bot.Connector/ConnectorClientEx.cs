// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// ConnectorClient extension
    /// </summary>
    public partial class ConnectorClient
    {
        /// <summary>
        /// Create a new instance of the ConnectorClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service</param>
        /// <param name="microsoftAppId">Optional. Your Microsoft app id. If null, this setting is read from settings["MicrosoftAppId"]</param>
        /// <param name="microsoftAppPassword">Optional. Your Microsoft app password. If null, this setting is read from settings["MicrosoftAppPassword"]</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, string microsoftAppId = null, string microsoftAppPassword = null, params DelegatingHandler[] handlers)
            : this(baseUri, new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword), handlers: handlers)
        {
        }

        private HttpClient _originalHttpClient;
        protected static Lazy<HttpClient> g_httpClient = new Lazy<HttpClient>(() =>
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Microsoft-BotFramework", "4.0"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue($"(BotBuilder .Net/{typeof(ConnectorClient).GetTypeInfo().Assembly.GetName().Version})"));
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            return httpClient;
        });

        public bool UseSharedHttpClient
        {
            get { return this._originalHttpClient != null; }
            set
            {
                if (value == false)
                {
                    this.HttpClient = this._originalHttpClient;
                }
                else
                {
                    if (this._originalHttpClient == null)
                    {
                        this._originalHttpClient = this.HttpClient;
                        this.HttpClient = g_httpClient.Value;
                    }
                }
            }
        }

        partial void CustomInitialize()
        {
            this.UseSharedHttpClient = true;
        }

        protected override void Dispose(bool disposing)
        {
            // replace global with original so dispose doesn't dispose the global one
            if (this._originalHttpClient != null)
            {
                this.HttpClient = this._originalHttpClient;
                this._originalHttpClient = null;
            }
            base.Dispose(disposing);
        }
    }
}
