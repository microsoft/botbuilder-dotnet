// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Security.Claims;
using System.Linq;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Extension methods for ClaimsIdentity
    /// </summary>
    public static class ClaimsIdentityEx
    {
        public const string AppPasswordClaim = "appPassword";
        public const string AppIdClaim= "appid";

        /// <summary>
        /// "azp" Claim. 
        /// From the OpenID Spec: 
        ///     http://openid.net/specs/openid-connect-core-1_0.html#IDToken      
        /// Authorized party - the party to which the ID Token was issued. If present, it MUST contain 
        /// the OAuth 2.0 Client ID of this party. This Claim is only needed when the ID Token has a 
        /// single audience value and that audience is different than the authorized party. It MAY be 
        /// included even when the authorized party is the same as the sole audience. The azp value is 
        /// a case sensitive string containing a StringOrURI value.
        /// </summary>
        public const string AuthorizedParty = "azp";

        /// <summary>
        /// Audiance Claim. From RFC 7519. 
        ///     https://tools.ietf.org/html/rfc7519#section-4.1.3
        /// The "aud" (audience) claim identifies the recipients that the JWT is
        /// intended for.  Each principal intended to process the JWT MUST
        /// identify itself with a value in the audience claim.If the principal
        /// processing the claim does not identify itself with a value in the
        /// "aud" claim when this claim is present, then the JWT MUST be
        /// rejected.In the general case, the "aud" value is an array of case-
        /// sensitive strings, each containing a StringOrURI value.In the
        /// special case when the JWT has one audience, the "aud" value MAY be a
        /// single case-sensitive string containing a StringOrURI value.The
        /// interpretation of audience values is generally application specific.
        /// Use of this claim is OPTIONAL.
        /// </summary>
        public const string AudienceClaim = "aud"; 
        
        /// <summary>
        /// Get the AppId from the Claims Identity
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetAppIdFromClaims(this ClaimsIdentity identity)
        {
            if (identity == null)
            {
                return null;
            }

            // emulator adds appid claim for v1 tokens, or azp for v2 tokens
            Claim botClaim = identity.Claims.FirstOrDefault(c => c.Type == AppIdClaim || c.Type == AuthorizedParty);
            if (botClaim != null)
            {
                return botClaim.Value;
            }

            // Fallback for BF-issued tokens
            botClaim = identity.Claims.FirstOrDefault(c => c.Issuer == "https://api.botframework.com" && c.Type == AudienceClaim);
            if (botClaim != null)
            {
                return botClaim.Value;
            }

            return null;
        }

        /// <summary>
        /// Get the AppPassword from the Claims Identity
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetAppPasswordFromClaims(this ClaimsIdentity identity)
        {
            return identity?.Claims.FirstOrDefault(c => c.Type == AppPasswordClaim)?.Value;
        }

        /// <summary>
        /// Get the MicrosoftAppCredentials using claims in the claims identity
        /// </summary>
        /// <param name="claimsIdentity"></param>
        /// <returns></returns>
        public static MicrosoftAppCredentials GetCredentialsFromClaims(this ClaimsIdentity claimsIdentity)
        {
            var appId = claimsIdentity.GetAppIdFromClaims();
            var password = claimsIdentity.GetAppPasswordFromClaims();
            return new MicrosoftAppCredentials(appId, password);
        }
    }
}
