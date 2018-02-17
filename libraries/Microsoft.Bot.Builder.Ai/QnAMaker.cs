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
        public const string qnaMakerServiceEndpoint = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases/";
        public const string APIManagementHeader = "Ocp-Apim-Subscription-Key";
        public const string JsonMimeType = "application/json";

        private readonly HttpClient _httpClient;
        private readonly QnAMakerOptions _options;
        private readonly string _answerUrl;

        public QnAMaker(QnAMakerOptions options, HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
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
        }

        public async Task<QueryResult[]> GetAnswers(string question)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _answerUrl);

            string jsonRequest = JsonConvert.SerializeObject(new
            {
                question,
                top = _options.Top
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
    }

    public class QueryResult
    {
        [JsonProperty("questions")]
        public string[] Questions { get; set; }

        [JsonProperty("answer")]
        public string Answer { get; set; }

        [JsonProperty("score")]
        public float Score { get; set; }
    }

    public class QueryResults
    {
        [JsonProperty("answers")]
        public QueryResult[] Answers { get; set; }
    }
}
