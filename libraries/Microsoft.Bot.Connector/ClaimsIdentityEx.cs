
namespace Microsoft.Bot.Connector
{
    using System.Security.Claims;
    using System.Linq;

    public static class ClaimsIdentityEx
    {
        public const string AppPasswordClaim = "appPassword";

        /// <summary>
        /// Get the AppId from the Claims Identity
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetAppIdFromClaims(this ClaimsIdentity identity)
        {
            if (identity == null)
                return null;

            // emulator adds appid claim
            Claim botClaim = identity.Claims.FirstOrDefault(c => c.Type == "appid");
            if (botClaim != null)
                return botClaim.Value;

            // Fallback for BF-issued tokens
            botClaim = identity.Claims.FirstOrDefault(c => c.Issuer == "https://api.botframework.com" && c.Type == "aud");
            if (botClaim != null)
                return botClaim.Value;

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
