// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai.QnA
{
    /// <summary>
    /// Provides access to a QnA Maker knowledge base.
    /// </summary>
    public class QnAMaker
    {
        /// <summary>
        /// The base service endpoint for QnA Maker.
        /// </summary>
        public const string qnaMakerServiceEndpoint = "https://westus.api.cognitive.microsoft.com/qnamaker/v3.0/knowledgebases/";

        /// <summary>
        /// The title for the HTTP header for the QnA Maker subscription key.
        /// </summary>
        public const string APIManagementHeader = "Ocp-Apim-Subscription-Key";

        /// <summary>
        /// The request content type.
        /// </summary>
        public const string JsonMimeType = "application/json";

        private static HttpClient g_httpClient = new HttpClient();
        private readonly HttpClient _httpClient;
        private readonly QnAMakerOptions _options;
        private readonly string _answerUrl;

        /// <summary>
        /// Creates a new <see cref="QnAMaker"/> instance.
        /// </summary>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">A client with which to talk to QnAMaker.
        /// If null, a default client is used for this instance.</param>
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

        /// <summary>
        /// Generates an answer from the knowledge base.
        /// </summary>
        /// <param name="question">The user question to be queried against your knowledge base.</param>
        /// <returns>A list of answers for the user query, sorted in decreasing order of ranking score.</returns>
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

    /// <summary>
    /// Defines options for the QnA Maker knowledge base.
    /// </summary>
    public class QnAMakerOptions
    {
        /// <summary>
        /// The subscription key for the knowledge base.
        /// </summary>
        public string SubscriptionKey { get; set; }

        /// <summary>
        /// The knowledge base ID.
        /// </summary>
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// The minimum score threshold, used to filter returned results.
        /// </summary>
        /// <remarks>Scores are normalized to the range of 0.0 to 1.0 
        /// before filtering.</remarks>
        public float ScoreThreshold { get; set; }

        /// <summary>
        /// The number of ranked results you want in the output.
        /// </summary>
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

    /// <summary>
    /// Represents an individual result from a knowledge base query.
    /// </summary>
    public class QueryResult
    {
        /// <summary>
        /// The list of questions indexed in the QnA Service for the given answer.
        /// </summary>
        [JsonProperty("questions")]
        public string[] Questions { get; set; }

        /// <summary>
        /// The answer text.
        /// </summary>
        [JsonProperty("answer")]
        public string Answer { get; set; }

        /// <summary>
        /// The answer's score, from 0.0 (least confidence) to
        /// 1.0 (greatest confidence).
        /// </summary>
        [JsonProperty("score")]
        public float Score { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public Metadata[] Metadata { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "qnaId")]
        public int QnaId { get; set; }
    }

    /// <summary>
    /// Contains answers for a user query.
    /// </summary>
    public class QueryResults
    {
        /// <summary>
        /// The answers for a user query,
        /// sorted in decreasing order of ranking score.
        /// </summary>
        [JsonProperty("answers")]
        public QueryResult[] Answers { get; set; }
    }
}
