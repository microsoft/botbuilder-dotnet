// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Configuration
{
    /// <summary>
    ///     A <see cref="ICredentialProvider">credential provider</see> which provides credentials
    ///     based on an <see cref="EndpointService">endpoint</see> from a <c>.bot</c> configuration file.
    /// </summary>
    /// <seealso cref="BotConfiguration"/>
    public sealed class BotConfigurationEndpointServiceCredentialProvider : ICredentialProvider
    {
        public static readonly string DefaultEnvironmentName = "development";

        private readonly EndpointService _endpointService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotConfigurationEndpointServiceCredentialProvider"/> class
        /// which exposes the credentials from the given <paramref name="endpointService">endpoint</paramref>.
        /// </summary>
        /// <param name="endpointService">The <see cref="EndpointService"/> instance containing the credentials that should be used.</param>
        public BotConfigurationEndpointServiceCredentialProvider(EndpointService endpointService)
        {
            _endpointService = endpointService ?? throw new ArgumentNullException(nameof(endpointService));
        }

        /// <summary>
        ///     Given an existing <see cref="BotConfiguration"/> instance, finds the endpoint service that corresponds to the
        /// <paramref name="environmentName">given environment</paramref>.
        /// </summary>
        /// <param name="botConfiguration">
        ///     An <see cref="BotConfiguration"/> instance containing one or more <see cref="EndpointService">endpoint services</see>.</param>
        /// <param name="environmentName">
        ///     Optional environment name that should be used to locate the correct <see cref="EndpointService">endpoint</see>
        ///     from the configuration file based on its <c>name</c>.
        /// </param>
        /// <returns>
        ///     An instance of a <see cref="BotConfigurationEndpointServiceCredentialProvider"/> loaded from the
        ///     <paramref name="botConfiguration">specified <see cref="BotConfiguration"/></paramref> populated
        ///     with the <see cref="EndpointService">endpoint</see> resolved based on the <paramref name="environmentName"/>.
        /// </returns>
        public static BotConfigurationEndpointServiceCredentialProvider FromConfiguration(BotConfiguration botConfiguration, string environmentName = null)
        {
            if (botConfiguration == null)
            {
                throw new ArgumentNullException(nameof(botConfiguration));
            }

            return new BotConfigurationEndpointServiceCredentialProvider(FindEndpointServiceForEnvironment(botConfiguration, environmentName ?? DefaultEnvironmentName));
        }

        /// <summary>
        ///     Attempts to load and create an <see cref="BotConfigurationEndpointServiceCredentialProvider"/> from the
        ///     by searching the <see cref="Environment.CurrentDirectory">current directory</see> for a <c>.bot</c> configuration file.
        /// </summary>
        /// <param name="botFileSecretKey">
        ///     Optional key that should be used to decrypt secrets inside the configuration file.
        /// </param>
        /// <param name="environmentName">
        ///     Optional environment name that should be used to locate the correct <see cref="EndpointService">endpoint</see>
        ///     from the configuration file based on its <c>name</c>.
        /// </param>
        /// <remarks>
        ///     If no value is provided for <paramref name="botFileSecretKey"/>, it is assumed that the contents of
        ///     the configuration file are not encrypted.
        ///
        ///     If no value is provided for <paramref name="environmentName"/>, it will use
        ///     <see cref="DefaultEnvironmentName">the default environment</see>.
        /// </remarks>
        /// <returns>
        ///     An instance of a <see cref="BotConfigurationEndpointServiceCredentialProvider"/> populated with the
        ///     <see cref="EndpointService">endpoint</see> resolved based on the <paramref name="environmentName"/>.
        /// </returns>
        /// <seealso cref="BotConfiguration"/>
        public static BotConfigurationEndpointServiceCredentialProvider Load(string botFileSecretKey = null, string environmentName = null) =>
            LoadBotConfiguration(() => BotConfiguration.LoadFromFolder(Environment.CurrentDirectory, botFileSecretKey), environmentName);

        /// <summary>
        /// Attempts to load and create an <see cref="BotConfigurationEndpointServiceCredentialProvider"/> from the
        /// <paramref name="botConfigurationFilePath">specified <c>.bot</c> configuration file</paramref>.
        /// </summary>
        /// <param name="botConfigurationFilePath">
        ///     A path to the .bot configuration file that should be loaded.
        /// </param>
        /// <param name="botFileSecretKey">
        ///     Optional key that should be used to decrypt secrets inside the configuration file.
        /// </param>
        /// <param name="environmentName">
        ///     Optional environment name that should be used to locate the correct <see cref="EndpointService">endpoint</see>
        ///     from the configuration file based on its <c>name</c>.
        /// </param>
        /// <remarks>
        ///     If no value is provided for <paramref name="botFileSecretKey"/>, it is assumed that the contents of
        ///     the configuration file are not encrypted.
        ///
        ///     If no value is provided for <paramref name="environmentName"/>, it will use
        ///     <see cref="DefaultEnvironmentName">the default environment</see>.
        /// </remarks>
        /// <returns>
        ///     An instance of a <see cref="BotConfigurationEndpointServiceCredentialProvider"/> loaded from the
        ///     <paramref name="botConfigurationFilePath">specified <c>.bot</c> configuration file</paramref> populated
        ///     with the <see cref="EndpointService">endpoint</see> resolved based on the <paramref name="environmentName"/>.
        /// </returns>
        /// <seealso cref="BotConfiguration"/>
        public static BotConfigurationEndpointServiceCredentialProvider LoadFrom(string botConfigurationFilePath, string botFileSecretKey = null, string environmentName = null)
        {
            if (string.IsNullOrEmpty(botConfigurationFilePath))
            {
                throw new ArgumentException(nameof(botConfigurationFilePath));
            }

            return LoadBotConfiguration(() => BotConfiguration.Load(botConfigurationFilePath, botFileSecretKey), environmentName);
        }

        /// <inheritdoc />
        public Task<string> GetAppPasswordAsync(string appId) => Task.FromResult<string>(_endpointService.AppPassword);

        /// <inheritdoc />
        public Task<bool> IsAuthenticationDisabledAsync() => Task.FromResult(string.IsNullOrWhiteSpace(_endpointService.AppId) && _endpointService.Name.Equals("development", StringComparison.InvariantCultureIgnoreCase));

        /// <inheritdoc />
        public Task<bool> IsValidAppIdAsync(string appId) => Task.FromResult(appId == _endpointService.AppId);

        private static BotConfigurationEndpointServiceCredentialProvider LoadBotConfiguration(Func<BotConfiguration> loader, string environmentName)
        {
            var botConfiguration = default(BotConfiguration);

            try
            {
                botConfiguration = loader();
            }
            catch (Exception exception)
            {
                throw new Exception(
                    @"Error reading .bot file; check inner exception for more details. Please ensure you have valid botFilePath and botFileSecret set for your environment.
        - You can find the botFilePath and botFileSecret in the Azure App Service application settings.
        - If you are running this bot locally, consider adding a appsettings.json file with botFilePath and botFileSecret.
        - See https://aka.ms/about-bot-file to learn more about .bot file its use and bot configuration.
        ",
                    exception);
            }

            return new BotConfigurationEndpointServiceCredentialProvider(FindEndpointServiceForEnvironment(botConfiguration, environmentName));
        }

        private static EndpointService FindEndpointServiceForEnvironment(BotConfiguration botConfiguration, string environmentName)
        {
            environmentName = environmentName ?? DefaultEnvironmentName;

            var endpointServiceForEnvironment = botConfiguration.Services.OfType<EndpointService>().FirstOrDefault(s => s.Name.Equals(environmentName, StringComparison.InvariantCultureIgnoreCase));

            if (endpointServiceForEnvironment == null)
            {
                throw new InvalidOperationException($"The .bot file does not appear to contain an endpoint service with the name \"{environmentName}\".");
            }

            return endpointServiceForEnvironment;
        }
    }
}
