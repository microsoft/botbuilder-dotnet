namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Configuration for OAuth client credential authentication.
    /// </summary>
    public class OAuthConfiguration
    {
        /// <summary>
        /// Gets or sets oAuth Authority for authentication.
        /// </summary>
        /// <value>
        /// OAuth Authority for authentication.
        /// </value>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets oAuth scope for authentication.
        /// </summary>
        /// <value>
        /// OAuth scope for authentication.
        /// </value>
        public string Scope { get; set; }
    }
}
