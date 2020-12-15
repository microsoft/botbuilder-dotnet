// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ICredentials = Microsoft.Bot.Connector.Authentication.ICredentialProvider;

namespace Microsoft.Bot.Builder.Runtime.Providers.Credentials
{
    /// <summary>
    /// Defines an implementation of <see cref="ICredentialProvider"/> that registers
    /// <see cref="SimpleCredentialProvider"/> with the application's service collection.
    /// </summary>
    [JsonObject]
    internal class DeclarativeCredentialsProvider : ICredentialProvider
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.DeclarativeCredentialsProvider";

        /// <summary>
        /// Gets or sets the application ID.
        /// </summary>
        /// <value>
        /// The application ID.
        /// </value>
        [JsonProperty("applicationId")]
        public StringExpression ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the application password.
        /// </summary>
        /// <value>
        /// The application password.
        /// </value>
        [JsonProperty("applicationPassword")]
        public StringExpression ApplicationPassword { get; set; }

        /// <summary>
        /// Register services with the application's service collection.
        /// </summary>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">Application configuration.</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddSingleton<ICredentials>(_ => new SimpleCredentialProvider(
                appId: this.ApplicationId?.GetConfigurationValue(configuration),
                password: this.ApplicationPassword?.GetConfigurationValue(configuration)));
        }
    }
}
