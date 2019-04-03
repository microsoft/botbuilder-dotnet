namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Represents the Id of a bot for a credential provider.
    /// </summary>
    public interface IAppId
    {
        /// <summary>
        /// Gets or sets the app ID for a credential provider.
        /// </summary>
        /// <value>
        /// The app ID for the credential.
        /// </value>
        string AppId { get; set; }
    }
}
