// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// ConnectorClient extension.
    /// </summary>
    public partial class ConnectorClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service.</param>
        /// <param name="microsoftAppId">Optional. Your Microsoft app id. If null, this setting is read from settings["MicrosoftAppId"].</param>
        /// <param name="microsoftAppPassword">Optional. Your Microsoft app password. If null, this setting is read from settings["MicrosoftAppPassword"].</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, string microsoftAppId = null, string microsoftAppPassword = null, params DelegatingHandler[] handlers)
            : this(baseUri, new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword), handlers: handlers)
        {
            AddDefaultRequestHeaders(HttpClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service.</param>
        /// <param name="credentials">Credentials for the Connector service.</param>
        /// <param name="addJwtTokenRefresher">(DEPRECATED).</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, MicrosoftAppCredentials credentials, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
            : this(baseUri, credentials, null, addJwtTokenRefresher, handlers)
        {
            AddDefaultRequestHeaders(HttpClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service.</param>
        /// <param name="credentials">Credentials for the Connector service.</param>
        /// <param name="customHttpClient">The HTTP client to be used by the connector client.</param>
        /// <param name="addJwtTokenRefresher">(DEPRECATED).</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, MicrosoftAppCredentials credentials, HttpClient customHttpClient, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
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
        /// Create a new instance of the ConnectorClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service</param>
        /// <param name="credentials">Credentials for the Connector service</param>
        /// <param name="customHttpClient">The HTTP client to be used by the connector client.</param>
        /// <param name="addJwtTokenRefresher">(DEPRECATED)</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, ServiceClientCredentials credentials, HttpClient customHttpClient, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
            : this(baseUri, handlers)
        {
            this.Credentials = credentials;
            if (customHttpClient != null)
            {
                this.HttpClient = customHttpClient;
            }
        }

        /// <summary>
        /// Create a new instances of the ConnectorClient.
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service.</param>
        /// <param name="credentials">Credentials for the Connector service.</param>
        /// <param name="httpClientHandler">The HTTP client message handler to be used by the connector client.</param>
        /// <param name="addJwtTokenRefresher">(DEPRECATED).</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, MicrosoftAppCredentials credentials, HttpClientHandler httpClientHandler, bool addJwtTokenRefresher = true, HttpClient customHttpClient = null, params DelegatingHandler[] handlers)
            : this(baseUri, httpClientHandler, handlers)
        {
            this.Credentials = credentials;
            if (customHttpClient != null)
            {
                this.HttpClient = customHttpClient;
                AddDefaultRequestHeaders(HttpClient);
            }
        }

        /// <summary>Gets a description of the operating system of the Azure Bot Service.</summary>
        /// <returns>A description of the operating system of the Azure Bot Service.</returns>
        public static string GetOsVersion()
        {
            return System.Runtime.InteropServices.RuntimeInformation.OSDescription;
        }

        /// <summary>Gets the platform architecture of the Azure Bot Service.</summary>
        /// <returns>The platform architecture of the Azure Bot Service.</returns>
        public static string GetArchitecture()
        {
            return System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString();
        }

        /// <summary>Gets the name of the .NET Framework version of the Azure Bot Service..</summary>
        /// <returns>The name of the .NET Framework version of the Azure Bot Service.</returns>
        public static string GetASPNetVersion()
        {
            return Assembly
                    .GetEntryAssembly()?
                    .GetCustomAttribute<TargetFrameworkAttribute>()?
                    .FrameworkName;
        }

        /// <summary>Gets the assembly version for the Azure Bot Service.</summary>
        /// <typeparam name="T">A type that implement ServiceClient.</typeparam>
        /// <param name="client">Client.</param>
        /// <returns>The assembly version for the Azure Bot Service.</returns>
        public static string GetClientVersion<T>(T client)
            where T : ServiceClient<T>
        {
            var type = client.GetType();
            var assembly = type.GetTypeInfo().Assembly;
            return assembly.GetName().Version.ToString();
        }

        public static void AddDefaultRequestHeaders(HttpClient httpClient)
        {
            // The Schema version is 3.1, put into the Microsoft-BotFramework header
            var botFwkProductInfo = new ProductInfoHeaderValue("Microsoft-BotFramework", "3.1");
            if (!httpClient.DefaultRequestHeaders.UserAgent.Contains(botFwkProductInfo))
            {
                httpClient.DefaultRequestHeaders.UserAgent.Add(botFwkProductInfo);
            }

            // The Client SDK Version
            //  https://github.com/Microsoft/botbuilder-dotnet/blob/d342cd66d159a023ac435aec0fdf791f93118f5f/doc/UserAgents.md
            var botBuilderProductInfo = new ProductInfoHeaderValue("BotBuilder", GetClientVersion());
            if (!httpClient.DefaultRequestHeaders.UserAgent.Contains(botBuilderProductInfo))
            {
                httpClient.DefaultRequestHeaders.UserAgent.Add(botBuilderProductInfo);
            }

            // Additional Info.
            // https://github.com/Microsoft/botbuilder-dotnet/blob/d342cd66d159a023ac435aec0fdf791f93118f5f/doc/UserAgents.md
            var userAgent = $"({GetASPNetVersion()}; {GetOsVersion()}; {GetArchitecture()})";
            if (ProductInfoHeaderValue.TryParse(userAgent, out var additionalProductInfo))
            {
                if (!httpClient.DefaultRequestHeaders.UserAgent.Contains(additionalProductInfo))
                {
                    httpClient.DefaultRequestHeaders.UserAgent.Add(additionalProductInfo);
                }
            }

            httpClient.DefaultRequestHeaders.ExpectContinue = false;
        }

        private static string GetClientVersion()
        {
            var type = typeof(ConnectorClient).GetType();
            var assembly = type.GetTypeInfo().Assembly;
            return assembly.GetName().Version.ToString();
        }

        partial void CustomInitialize()
        {
            // Override the contract resolver with the Default because we want to be able to serialize annonymous types
            SerializationSettings.ContractResolver = new DefaultContractResolver();
            DeserializationSettings.ContractResolver = new DefaultContractResolver();
        }
    }
}
