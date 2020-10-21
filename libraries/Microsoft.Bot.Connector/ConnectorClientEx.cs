// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Implements a client for the Bot Connector service.
    /// </summary>
    public partial class ConnectorClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Bot Connector service.</param>
        /// <param name="microsoftAppId">Optional, the Microsoft app ID for the bot resource.
        /// If null, this setting is read from the `MicrosoftAppId` setting for the bot's application resource.</param>
        /// <param name="microsoftAppPassword">Optional, the Microsoft app password for the bot.
        /// If null, this setting is read from the `MicrosoftAppPassword` setting for the bot's application resource.</param>
        /// <param name="handlers">Optional, an array of <see cref="DelegatingHandler"/> objects to
        /// add to the HTTP client pipeline.</param>
        public ConnectorClient(Uri baseUri, string microsoftAppId = null, string microsoftAppPassword = null, params DelegatingHandler[] handlers)
            : this(baseUri, new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword), handlers: handlers)
        {
            AddDefaultRequestHeaders(HttpClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Bot Connector service.</param>
        /// <param name="credentials">Credentials for the Bot Connector service.</param>
        /// <param name="addJwtTokenRefresher">Deprecated, do not use.</param>
        /// <param name="handlers">Optional, an array of <see cref="DelegatingHandler"/> objects to
        /// add to the HTTP client pipeline.</param>
        public ConnectorClient(Uri baseUri, MicrosoftAppCredentials credentials, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
            : this(baseUri, credentials, null, addJwtTokenRefresher, handlers)
        {
            AddDefaultRequestHeaders(HttpClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Bot Connector service.</param>
        /// <param name="credentials">Credentials for the Bot Connector service.</param>
        /// <param name="addJwtTokenRefresher">Deprecated, do not use.</param>
        /// <param name="customHttpClient">The HTTP client to use for this connector client.</param>
        /// <param name="handlers">Optional, an array of <see cref="DelegatingHandler"/> objects to
        /// add to the HTTP client pipeline.</param>
        public ConnectorClient(Uri baseUri, MicrosoftAppCredentials credentials, HttpClient customHttpClient, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
            : this(baseUri, credentials as ServiceClientCredentials, customHttpClient, addJwtTokenRefresher, handlers)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Bot Connector service.</param>
        /// <param name="credentials">Credentials for the Bot Connector service.</param>
        /// <param name="customHttpClient">The HTTP client to use for this connector client.</param>
        /// <param name="disposeHttpClient">Whether to dispose the <see cref="HttpClient"/>.</param>
        /// <remarks>Constructor specifically designed to be the one that allows control of the disposing of the custom <see cref="HttpClient"/>.
        /// <see cref="ServiceClient{T}"/> only has one constructor that accepts control of the disposing of the <see cref="HttpClient"/>, so we call that overload here.
        /// All other overloads of <see cref="ConnectorClient"/> will not control this parameter and it will default to true, resulting on disposal of the provided <see cref="HttpClient"/> when the <see cref="ConnectorClient"/> is disposed.
        /// When reusing <see cref="HttpClient"/> instances across connectors, pass 'false' for <paramref name="disposeHttpClient"/> to avoid <see cref="ObjectDisposedException"/>.</remarks>
#pragma warning disable CA1801 // Review unused parameters (we can't change this without breaking binary compat)
        public ConnectorClient(Uri baseUri, ServiceClientCredentials credentials, HttpClient customHttpClient, bool disposeHttpClient)
#pragma warning restore CA1801 // Review unused parameters
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Bot Connector service.</param>
        /// <param name="credentials">Credentials for the Bot Connector service.</param>
        /// <param name="addJwtTokenRefresher">Deprecated, do not use.</param>
        /// <param name="customHttpClient">The HTTP client to use for this connector client.</param>
        /// <param name="handlers">Optional, an array of <see cref="DelegatingHandler"/> objects to
        /// add to the HTTP client pipeline.</param>
#pragma warning disable CA1801 // Review unused parameters (we can't change this without breaking binary compat)
        public ConnectorClient(Uri baseUri, ServiceClientCredentials credentials, HttpClient customHttpClient, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
#pragma warning restore CA1801 // Review unused parameters
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
        /// Initializes a new instance of the <see cref="ConnectorClient"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the Bot Connector service.</param>
        /// <param name="credentials">Credentials for the Bot Connector service.</param>
        /// <param name="httpClientHandler">The HTTP client message handler to use for this connector client.</param>
        /// <param name="addJwtTokenRefresher">Deprecated, do not use.</param>
        /// <param name="customHttpClient">The HTTP client to use for this connector client.</param>
        /// <param name="handlers">Optional, an array of <see cref="DelegatingHandler"/> objects to
        /// add to the HTTP client pipeline.</param>
#pragma warning disable CA1801 // Review unused parameters (we can't remove the addJwtTokenRefresher parameter without breaking binary compat)
        public ConnectorClient(Uri baseUri, MicrosoftAppCredentials credentials, HttpClientHandler httpClientHandler, bool addJwtTokenRefresher = true, HttpClient customHttpClient = null, params DelegatingHandler[] handlers)
#pragma warning restore CA1801 // Review unused parameters
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
                    .FrameworkName ?? RuntimeInformation.FrameworkDescription;
        }

        /// <summary>Gets the assembly version for the Azure Bot Service.</summary>
        /// <typeparam name="T">The type of REST service client to get the version of.</typeparam>
        /// <param name="client">The REST service client instance to get the version of.</param>
        /// <returns>The assembly version for the Azure Bot Service.</returns>
        public static string GetClientVersion<T>(T client)
            where T : ServiceClient<T>
        {
            var type = client.GetType();
            var assembly = type.GetTypeInfo().Assembly;
            return assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Configures an HTTP client to include default headers for the Bot Framework.
        /// </summary>
        /// <param name="httpClient">The HTTP client to configure.</param>
        public static void AddDefaultRequestHeaders(HttpClient httpClient)
        {
            lock (httpClient)
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
        }

        /// <summary>
        /// Gets the assembly version for this Bot Connector client.
        /// </summary>
        /// <returns>The assembly version for this Bot Connector client.</returns>
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
