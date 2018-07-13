namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;

    /// <summary>
    /// The definition of a particular connection setting for a bot
    /// </summary>
    public class ConnectionSetting
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("serviceProviderId")]
        public string ServiceProviderId { get; set; }

        [JsonProperty("serviceProviderDisplayName")]
        public string ServiceProviderDisplayName { get; set; }
    }
}
