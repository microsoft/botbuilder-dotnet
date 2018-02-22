// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai
{
    public class QnAMaker
    {
        public const string qnaMakerServiceEndpoint = "https://westus.api.cognitive.microsoft.com/qnamaker/v3.0/knowledgebases/";
        public const string APIManagementHeader = "Ocp-Apim-Subscription-Key";
        public const string JsonMimeType = "application/json";

        private static HttpClient g_httpClient = new HttpClient();
        private readonly HttpClient _httpClient;
        private readonly QnAMakerOptions _options;
        private readonly string _answerUrl;

        public QnAMaker(QnAMakerOptions options, HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? g_httpClient;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrEmpty(options.KnowledgeBaseId))
            {
                throw new ArgumentException(nameof(options.KnowledgeBaseId));
            }

            _answerUrl = $"{qnaMakerServiceEndpoint}{options.KnowledgeBaseId}/generateanswer";

            if (_options.ScoreThreshold == 0)
            {
                _options.ScoreThreshold = 0.3F;
            }

            if (_options.Top == 0)
            {
                _options.Top = 1;
            }

            if (_options.StrictFilters == null)
            {
                _options.StrictFilters = new Metadata[] {};
            }

            if (_options.MetadataBoost == null)
            {
                _options.MetadataBoost = new Metadata[] { };
            }
        }

        public async Task<QueryResult[]> GetAnswers(string question)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _answerUrl);

            string jsonRequest = JsonConvert.SerializeObject(new
            {
                question,
                top = _options.Top,
                strictFilters = _options.StrictFilters,
                metadataBoost = _options.MetadataBoost
            }, Formatting.None);

            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, JsonMimeType);
            content.Headers.Add(APIManagementHeader, _options.SubscriptionKey);

            var response = await _httpClient.PostAsync(_answerUrl, content).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var results = JsonConvert.DeserializeObject<QueryResults>(jsonResponse);
                foreach (var answer in results.Answers)
                {
                    answer.Score = answer.Score / 100;
                }

                return results.Answers.Where(answer => answer.Score > _options.ScoreThreshold).ToArray();
            }
            return null;
        }
    }

    public class QnAMakerOptions
    {
        public string SubscriptionKey { get; set; }
        public string KnowledgeBaseId { get; set; }
        public float ScoreThreshold { get; set; }
        public int Top { get; set; }
        public Metadata[] StrictFilters { get; set; }
        public Metadata[] MetadataBoost { get; set; }
    }

    [Serializable]
    public class Metadata
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }

    public class QueryResult
    {
        [JsonProperty("questions")]
        public string[] Questions { get; set; }

        [JsonProperty("answer")]
        public string Answer { get; set; }

        [JsonProperty("score")]
        public float Score { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public Metadata[] Metadata { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "qnaId")]
        public int QnaId { get; set; }
    }

    public class QueryResults
    {
        [JsonProperty("answers")]
        public QueryResult[] Answers { get; set; }
    }
}
