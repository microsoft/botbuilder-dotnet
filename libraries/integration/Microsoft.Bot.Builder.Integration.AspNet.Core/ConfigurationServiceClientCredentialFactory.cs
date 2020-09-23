// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Credential provider which uses <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> to lookup appId and password.
    /// </summary>
    /// <remarks>
    /// This will populate the <see cref="PasswordServiceClientCredentialFactory.AppId"/> from an configuration entry with the key of <see cref="MicrosoftAppCredentials.MicrosoftAppIdKey"/>
    /// and the <see cref="PasswordServiceClientCredentialFactory.Password"/> from a configuration entry with the key of <see cref="MicrosoftAppCredentials.MicrosoftAppPasswordKey"/>.
    ///
    /// NOTE: if the keys are not present, a <c>null</c> value will be used.
    /// </remarks>
    public class ConfigurationServiceClientCredentialFactory : PasswordServiceClientCredentialFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationServiceClientCredentialFactory"/> class.
        /// </summary>
        /// <param name="configuration">An instance of <see cref="IConfiguration"/>.</param>
        /// <param name="httpClient">A httpClient to use.</param>
        /// <param name="logger">A logger to use.</param>
        public ConfigurationServiceClientCredentialFactory(IConfiguration configuration, HttpClient httpClient = null, ILogger logger = null)
            : base(
            configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value,
            configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value,
            httpClient,
            logger)
        {
        }
    }
}
