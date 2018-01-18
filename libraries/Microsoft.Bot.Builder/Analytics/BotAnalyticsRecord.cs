using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Microsoft.Bot.Builder.Analytics
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class BotAnalyticsRecord  : FlexObject    
    {        
        [JsonProperty("@id")]
        public string id { get; set; }
        [JsonProperty("@type")]
        public string Type { get; set; } = "botAnalyticsRecord";
        [JsonProperty("@context")]
        public string Context { get; set; } = "http://www.microsoft.com/botFramework/schemas/analytics/v1";

        public string BotId { get; set; }

        public DateTime ReceivedAtDateTime { get; set; }
    }
    
    public interface IAnalyticsFacet
    {
        string FacetName { get; }
    }
}
