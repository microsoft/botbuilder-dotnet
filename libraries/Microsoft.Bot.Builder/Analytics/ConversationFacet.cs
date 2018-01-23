// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Analytics
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ConversationFacet : FlexObject, IAnalyticsFacet
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "botConversation";        
        public string ConversationId { get; set; }        
        public int Turn { get; set; } = 0;

        [JsonIgnore]
        public string FacetName => "conversation";         
    }

    public static class AnalyticsFacetExtensions
    {
        public static void AddToAnalyticsRecord(this IAnalyticsFacet facet, BotAnalyticsRecord record)
        {
            record[facet.FacetName] = facet; 
        }
    }
}