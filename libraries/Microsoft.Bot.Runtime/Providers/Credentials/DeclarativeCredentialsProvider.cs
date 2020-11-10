// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ICredentials = Microsoft.Bot.Connector.Authentication.ICredentialProvider;

namespace Microsoft.Bot.Runtime.Providers.Credentials
{
    [JsonObject]
    public class DeclarativeCredentialsProvider : ICredentialProvider
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.DeclarativeCredentialsProvider";

        [JsonProperty("applicationId")]
        public StringExpression ApplicationId { get; set; }

        [JsonProperty("applicationPassword")]
        public StringExpression ApplicationPassword { get; set; }

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
