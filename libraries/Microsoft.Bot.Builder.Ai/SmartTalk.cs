using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;

namespace Microsoft.Bot.Builder.Ai
{
    public class SmartTalk
    {
        public const string smartTalkServiceEndPoint = "https://smarttalkmanagementservice.azure-api.net/smarttalk/api/SmartTalk/GetResponse";
        public const string APIManagementHeader = "Ocp-Apim-Subscription-Key";
        public const string JsonMimeType = "application/json";

        private static HttpClient g_httpClient = new HttpClient();
        private readonly HttpClient _httpClient;
        private readonly SmartTalkMiddlewareOptions _options;

        public SmartTalk(SmartTalkMiddlewareOptions options, HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? g_httpClient;
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<SmartTalkResults> GetAnswers(string query)
        {
            var requestObject = new SmartTalkRequest(query, _options.BotPersona);

            string jsonRequest = JsonConvert.SerializeObject(requestObject, Formatting.None);

            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, JsonMimeType);
            content.Headers.Add(APIManagementHeader, _options.SubscriptionKey);

            var response = await _httpClient.PostAsync(smartTalkServiceEndPoint, content).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var results = JsonConvert.DeserializeObject<SmartTalkResults>(jsonResponse);

                results.ScenarioList = results.ScenarioList.Where(scenario => scenario.Score > _options.ScoreThreshold).ToList();

                return results;
            }

            return null;
        }
    }

    public enum Persona
    {
        Friendly,
        Professional
    }

    public class SmartTalkRequest
    {
        public SmartTalkRequest(string query, Persona persona)
        {
            this.Query = query;
            this.Persona = persona;
        }

        public string Query { get; set; }

        public Persona Persona { get; set; }
    }

    [Serializable]
    public class SmartTalkResults
    {
        [JsonProperty(PropertyName = "ScenarioList")]
        public List<SmartTalkResultsScenario> ScenarioList { get; set; }

        [JsonProperty(PropertyName = "IsChatQuery")]
        public bool IsChatQuery { get; set; } = false;
    }

    [Serializable]
    public class SmartTalkResultsScenario
    {
        [JsonProperty(PropertyName = "ScenarioName")]
        public string ScenarioName { get; set; }

        [JsonProperty(PropertyName = "Score")]
        public double Score { get; set; }

        [JsonProperty(PropertyName = "Responses")]
        public List<string> Responses { get; set; }
    }
}
