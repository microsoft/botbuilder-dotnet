// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Teams
{
    /// <summary>
    /// Implements a client for the Teams Bot Connector service.
    /// </summary>
    public partial class TeamsConnectorClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Bot Connector service.</param>
        /// <param name="credentials">Credentials for the Bot Connector service.</param>
        /// <param name="customHttpClient">The HTTP client to use for this connector client.</param>
        /// <param name="handlers">Optional, an array of <see cref="DelegatingHandler"/> objects to
        /// add to the HTTP client pipeline.</param>
        public TeamsConnectorClient(Uri baseUri, ServiceClientCredentials credentials, HttpClient customHttpClient, params DelegatingHandler[] handlers)
            : this(baseUri, handlers)
        {
            this.Credentials = credentials;
            if (customHttpClient != null)
            {
                this.HttpClient = customHttpClient;

                // Note don't call AddDefaultRequestHeaders(HttpClient) here because the BotFrameworkAdapter
                // called it. Updating DefaultRequestHeaders is not thread safe this is OK because the
                // adapter should be a singleton.
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Bot Connector service.</param>
        /// <param name="credentials">Credentials for the Bot Connector service.</param>
        /// <param name="customHttpClient">The HTTP client to use for this connector client.</param>
        /// <param name="disposeHttpClient">Whether to dispose the <see cref="HttpClient"/>.</param>
        /// <remarks>Constructor specifically designed to be the one that allows control of the disposing of the custom <see cref="HttpClient"/>.
        /// <see cref="ServiceClient{T}"/> only has one constructor that accepts control of the disposing of the <see cref="HttpClient"/>, so we call that overload here.
        /// All other overloads of <see cref="ConnectorClient"/> will not control this parameter and it will default to true, resulting on disposal of the provided <see cref="HttpClient"/> when the <see cref="ConnectorClient"/> is disposed.
        /// When reusing <see cref="HttpClient"/> instances across connectors, pass 'false' for <paramref name="disposeHttpClient"/> to avoid <see cref="ObjectDisposedException"/>.</remarks>
        public TeamsConnectorClient(Uri baseUri, ServiceClientCredentials credentials, HttpClient customHttpClient, bool disposeHttpClient)
            : base(customHttpClient, disposeHttpClient)
        {
            this.Credentials = credentials;

            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            Initialize();

            BaseUri = baseUri;
        }
    }
}
