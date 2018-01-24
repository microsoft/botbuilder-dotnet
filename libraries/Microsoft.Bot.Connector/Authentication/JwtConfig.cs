// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Configuration for JWT tokens
    /// </summary>
    public static class JwtConfig
    {
        /// <summary>
        /// TO CHANNEL FROM BOT: Login URL
        /// </summary>
        public const string ToChannelFromBotLoginUrl = "https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token";

        /// <summary>
        /// TO CHANNEL FROM BOT: OAuth scope to request
        /// </summary>
        public const string ToChannelFromBotOAuthScope = "https://api.botframework.com/.default";

        /// <summary>
        /// TO BOT FROM CHANNEL: OpenID metadata document for tokens coming from MSA
        /// </summary>
        public const string ToBotFromChannelOpenIdMetadataUrl = "https://login.botframework.com/v1/.well-known/openidconfiguration";


        /// <summary>
        /// TO BOT FROM CHANNEL: Allowed token signing algorithms
        /// </summary>
        public static readonly string[] ToBotFromChannelAllowedSigningAlgorithms = new[] { "RS256", "RS384", "RS512" };

        /// <summary>
        /// TO BOT FROM EMULATOR: OpenID metadata document for tokens coming from MSA
        /// </summary>
        public const string ToBotFromEmulatorOpenIdMetadataUrl = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

    }
}
