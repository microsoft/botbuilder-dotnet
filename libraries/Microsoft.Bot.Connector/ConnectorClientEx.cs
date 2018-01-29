// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
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

        /// <summary>
        /// client defaults to sending the expect: continue header, which isn't very efficient,
        /// </summary>
        partial void CustomInitialize()
        {
            AddUserAgent(this);
            HttpClient.DefaultRequestHeaders.ExpectContinue = false;
        }

        internal static void AddUserAgent<T>(T client) where T : ServiceClient<T>
        {
            client.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Microsoft-BotFramework", "4.0"));
            client.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue($"(BotBuilder .Net/{GetClientVersion(client)})"));
        }

        internal static string GetClientVersion<T>(T client) where T : ServiceClient<T>
        {
            var type = client.GetType();
            var assembly = type.GetTypeInfo().Assembly;
            return assembly.GetName().Version.ToString();
        }
    }
}
