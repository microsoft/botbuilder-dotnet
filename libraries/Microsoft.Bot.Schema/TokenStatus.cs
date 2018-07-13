namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;

    /// <summary>
    /// The status of a particular token
    /// </summary>
    public class TokenStatus
    {
        /// <summary>
        /// The name of the connection the token status pertains to
        /// </summary>
        [JsonProperty("connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// The value of the status (int or TokenStatusValue)
        /// 0 = No Token
        /// 1 = Token
        /// </summary>
        [JsonProperty("status")]
        public TokenStatusValue Status { get; set; }

        /// <summary>
        /// The display name of the service provider for which this Token belongs to
        /// </summary>
        [JsonProperty("serviceProviderDisplayName")]
        public string ServiceProviderDisplayName { get; set; }
    }
}
