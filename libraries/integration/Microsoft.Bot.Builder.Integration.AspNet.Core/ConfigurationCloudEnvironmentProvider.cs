// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Creates a cloud environment instance from configuration.
    /// </summary>
    public class ConfigurationCloudEnvironmentProvider : ICloudEnvironmentProvider
    {
        private string _channelService;
        private string _toChannelFromBotLoginUrl;
        private string _toChannelFromBotOAuthScope;
        private string _toBotFromChannelTokenIssuer;
        private string _oAuthUrl;
        private string _toBotFromChannelOpenIdMetadataUrl;
        private string _toBotFromEmulatorOpenIdMetadataUrl;
        private string _callerId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationCloudEnvironmentProvider"/> class.
        /// </summary>
        /// <param name="configuration">An IConfiguration instance.</param>
        public ConfigurationCloudEnvironmentProvider(IConfiguration configuration)
        {
            _channelService = configuration.GetSection("ChannelService")?.Value;
            _toChannelFromBotLoginUrl = configuration.GetSection("ToChannelFromBotLoginUrl")?.Value;
            _toChannelFromBotOAuthScope = configuration.GetSection("ToChannelFromBotOAuthScope")?.Value;
            _toBotFromChannelTokenIssuer = configuration.GetSection("ToBotFromChannelTokenIssuer")?.Value;
            _oAuthUrl = configuration.GetSection("OAuthUrl")?.Value;
            _toBotFromChannelOpenIdMetadataUrl = configuration.GetSection("ToBotFromChannelOpenIdMetadataUrl")?.Value;
            _toBotFromEmulatorOpenIdMetadataUrl = configuration.GetSection("ToBotFromEmulatorOpenIdMetadataUrl")?.Value;
            _callerId = configuration.GetSection("CallerId")?.Value;
        }

        /// <summary>
        /// Creates the appropriate cloud environment according to the configuration.
        /// </summary>
        /// <returns>The ICloudEnvironment instance.</returns>
        public Task<ICloudEnvironment> GetCloudEnvironmentAsync()
        {
            var cloudEnvironment = CloudEnvironment.Create(
                _channelService,
                _toChannelFromBotLoginUrl,
                _toChannelFromBotOAuthScope,
                _toBotFromChannelTokenIssuer,
                _oAuthUrl,
                _toBotFromChannelOpenIdMetadataUrl,
                _toBotFromEmulatorOpenIdMetadataUrl,
                _callerId);

            return Task.FromResult(cloudEnvironment);
        }
    }
}
