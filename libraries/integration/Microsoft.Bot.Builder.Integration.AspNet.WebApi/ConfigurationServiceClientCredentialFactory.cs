// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Configuration;
using System.Net.Http;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    /// <summary>
    /// Credential provider which uses Configuration to lookup appId and password.
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
        /// <param name="httpClient">A httpClient to use.</param>
        /// <param name="logger">A logger to use.</param>
        public ConfigurationServiceClientCredentialFactory(HttpClient httpClient = null, ILogger logger = null)
            : base(
            ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppIdKey],
            ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppPasswordKey],
            httpClient,
            logger)
        {
        }
    }
}
