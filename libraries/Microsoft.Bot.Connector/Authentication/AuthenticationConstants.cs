// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Values and Constants used for Authentication and Authrization by the Bot Framework Protocol.
    /// </summary>
    public static class AuthenticationConstants
    {
        /// <summary>
        /// TO CHANNEL FROM BOT: Login URL.
        ///
        /// DEPRECATED. For binary compat only.
        /// </summary>
        public const string ToChannelFromBotLoginUrl = "https://login.microsoftonline.com/botframework.com";

        /// <summary>
        /// TO CHANNEL FROM BOT: Login URL template string. Bot developer may specify
        /// which tenant to obtain an access token from. By default, the channels only
        /// accept tokens from "botframework.com". For more details see https://aka.ms/bots/tenant-restriction.
        /// </summary>
        public const string ToChannelFromBotLoginUrlTemplate = "https://login.microsoftonline.com/{0}";

        /// <summary>
        /// The default tenant to acquire bot to channel token from.
        /// </summary>
        public const string DefaultChannelAuthTenant = "botframework.com";

        /// <summary>
        /// TO CHANNEL FROM BOT: OAuth scope to request.
        /// </summary>
        public const string ToChannelFromBotOAuthScope = "https://api.botframework.com";

        /// <summary>
        /// TO BOT FROM CHANNEL: Token issuer.
        /// </summary>
        public const string ToBotFromChannelTokenIssuer = "https://api.botframework.com";

        /// <summary>
        /// OAuth Url used to get a token from OAuthApiClient.
        /// </summary>
        public const string OAuthUrl = "https://api.botframework.com";

        /// <summary>
        /// Application Setting Key for the OpenIdMetadataUrl value.
        /// </summary>
        public const string BotOpenIdMetadataKey = "BotOpenIdMetadata";

        /// <summary>
        /// Application Setting Key for the OAuthUrl value.
        /// </summary>
        public const string OAuthUrlKey = "OAuthApiEndpoint";

        /// <summary>
        /// Application Settings Key for whether to emulate OAuthCards when using the emulator.
        /// </summary>
        public const string EmulateOAuthCardsKey = "EmulateOAuthCards";

        /// <summary>
        /// TO BOT FROM CHANNEL: OpenID metadata document for tokens coming from MSA.
        /// </summary>
        public const string ToBotFromChannelOpenIdMetadataUrl = "https://login.botframework.com/v1/.well-known/openidconfiguration";

        /// <summary>
        /// TO BOT FROM EMULATOR: OpenID metadata document for tokens coming from MSA.
        /// </summary>
        public const string ToBotFromEmulatorOpenIdMetadataUrl = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

        /// <summary>
        /// TO BOT FROM ENTERPRISE CHANNEL: OpenID metadata document for tokens coming from MSA.
        /// </summary>
        public const string ToBotFromEnterpriseChannelOpenIdMetadataUrlFormat = "https://{0}.enterprisechannel.botframework.com/v1/.well-known/openidconfiguration";

        /// <summary>
        /// "azp" Claim.
        /// Authorized party - the party to which the ID Token was issued.
        /// This claim follows the general format set forth in the OpenID Spec.
        ///     http://openid.net/specs/openid-connect-core-1_0.html#IDToken.
        /// </summary>
        public const string AuthorizedParty = "azp";

        /// <summary>
        /// Audience Claim. From RFC 7519.
        ///     https://tools.ietf.org/html/rfc7519#section-4.1.3
        /// The "aud" (audience) claim identifies the recipients that the JWT is
        /// intended for. Each principal intended to process the JWT MUST
        /// identify itself with a value in the audience claim. If the principal
        /// processing the claim does not identify itself with a value in the
        /// "aud" claim when this claim is present, then the JWT MUST be
        /// rejected. In the general case, the "aud" value is an array of case-
        /// sensitive strings, each containing a StringOrURI value. In the
        /// special case when the JWT has one audience, the "aud" value MAY be a
        /// single case-sensitive string containing a StringOrURI value. The
        /// interpretation of audience values is generally application specific.
        /// Use of this claim is OPTIONAL.
        /// </summary>
        public const string AudienceClaim = "aud";

        /// <summary>
        /// From RFC 7515
        ///     https://tools.ietf.org/html/rfc7515#section-4.1.4
        /// The "kid" (key ID) Header Parameter is a hint indicating which key
        /// was used to secure the JWS. This parameter allows originators to
        /// explicitly signal a change of key to recipients. The structure of
        /// the "kid" value is unspecified. Its value MUST be a case-sensitive
        /// string. Use of this Header Parameter is OPTIONAL.
        /// When used with a JWK, the "kid" value is used to match a JWK "kid"
        /// parameter value.
        /// </summary>
        public const string KeyIdHeader = "kid";

        /// <summary>
        /// Token version claim name. As used in Microsoft AAD tokens.
        /// </summary>
        public const string VersionClaim = "ver";

        /// <summary>
        /// App ID claim name. As used in Microsoft AAD 1.0 tokens.
        /// </summary>
        public const string AppIdClaim = "appid";

        /// <summary>
        /// Service URL claim name. As used in Microsoft Bot Framework v3.1 auth.
        /// </summary>
        public const string ServiceUrlClaim = "serviceurl";

        /// <summary>
        /// AppId used for creating skill claims when there is no appId and password configured.
        /// </summary>
        public const string AnonymousSkillAppId = "AnonymousSkill";

        /// <summary>
        /// Indicates that <see cref="ClaimsIdentity.AuthenticationType"/> is anonymous (no app Id and password were provided).
        /// </summary>
        public const string AnonymousAuthType = "anonymous";

        /// <summary>
        /// Allowed token signing algorithms. Tokens come from channels to the bot. The code
        /// that uses this also supports tokens coming from the emulator.
        /// </summary>
        public static readonly HashSet<string> AllowedSigningAlgorithms = new HashSet<string>(new[] { "RS256", "RS384", "RS512" });
    }
}
