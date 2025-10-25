// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Extension methods to add Bot Framework Authentication from configuration.
    /// </summary>
    public static class AddBotFrameworkAuthenticationFromConfiguration
    {
        /// <summary>
        /// Adds Bot Framework Authentication to the service collection from configuration.
        /// </summary>
        /// <param name="services">Services.</param>
        /// <param name="configuration">Configuration.</param>
        public static void AddBotFrameworkAuthFromConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTokenAcquisition(true);

            services.AddAgentIdentities();

            services.AddInMemoryTokenCaches();

            services.AddHttpClient();

            CredentialDescription credSecret;

            // Enable Federated Identity Credential
            if (!string.IsNullOrEmpty(configuration["MicrosoftAppClientId"])) 
            {
                services.AddSingleton<ServiceClientCredentialsFactory>(provider =>
                    new FederatedServiceClientCredentialsFactory(
                        provider.GetRequiredService<IAuthorizationHeaderProvider>(),
                        configuration["MicrosoftAppId"],
                        configuration["MicrosoftAppClientId"],
                        configuration["MicrosoftAppTenantId"]));

                // Set up to use Managed Identity
                credSecret = new CredentialDescription() 
                {
                    SourceType = CredentialSource.SignedAssertionFromManagedIdentity,
                    ManagedIdentityClientId = configuration["MicrosoftAppClientId"]
                };
            }

            // otherwise use Client Secret
            else
            {
                credSecret = new CredentialDescription()
                {
                    SourceType = CredentialSource.ClientSecret,
                    ClientSecret = configuration["MicrosoftAppPassword"]
                };
            }

            //TODO: Support more CredentialDescription types if needed, such as X509Certificate

            services.Configure<MicrosoftIdentityApplicationOptions>(ops =>
            {
                //TODO: Make configurable if needed
                ops.Instance = "https://login.microsoftonline.com/"; 
                ops.TenantId = configuration["MicrosoftAppTenantId"];
                ops.ClientId = configuration["MicrosoftAppId"];
                ops.ClientCredentials = new List<CredentialDescription> { credSecret };
            });

            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            services.AddSingleton<IBotFrameworkHttpAdapter>(provider =>
                new CloudAdapter(
                    provider.GetRequiredService<IAuthorizationHeaderProvider>(),
                    provider.GetRequiredService<IConfiguration>(),
                    provider.GetRequiredService<IHttpClientFactory>(),
                    provider.GetRequiredService<ILogger<CloudAdapter>>()));
        }
    }
}
