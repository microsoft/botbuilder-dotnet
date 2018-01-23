// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai
{

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


    public class QnAMakerOptions
    {
        public string SubscriptionKey { get; set; }
        public string KnowledgeBaseId { get; set; }
        public float ScoreThreshold { get; set; }
        public int Top { get; set; }
    }

    public class QnAMaker : Middleware.IReceiveActivity, IDisposable
    {
        public const string qnaMakerServiceEndpoint = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases/";
        private string answerUrl;
        private QnAMakerOptions options;
        private HttpClient httpClient;

        public QnAMaker(QnAMakerOptions options)
        {
            this.answerUrl = $"{qnaMakerServiceEndpoint}{options.KnowledgeBaseId}/generateanswer";
            if (options.ScoreThreshold == 0)
                options.ScoreThreshold = 0.3F;
            if (options.Top == 0)
                options.Top = 1;
            this.options = options;
        }

        public async Task<QueryResult[]> GetAnswers(string question)
        {
            lock (options)
            {
                if (httpClient == null)
                    this.httpClient = new HttpClient();
            }
            var request = new HttpRequestMessage(HttpMethod.Post, this.answerUrl);

            string jsonRequest = JsonConvert.SerializeObject(new
            {
                question,
                top = this.options.Top
            }, Formatting.None);

            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");
            content.Headers.Add("Ocp-Apim-Subscription-Key", this.options.SubscriptionKey);
            var response = await httpClient.PostAsync(this.answerUrl, content).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var results = JsonConvert.DeserializeObject<QueryResults>(jsonResponse);
                foreach (var answer in results.Answers)
                    answer.Score = answer.Score / 100;
                return results.Answers.Where(answer => answer.Score > this.options.ScoreThreshold).ToArray();
            }
            return null;
        }

        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                var results = await this.GetAnswers(context.Request.AsMessageActivity().Text.Trim()).ConfigureAwait(false);
                if (results.Any())
                {
                    context.Reply(results.First().Answer);             
                }
            }

            await next().ConfigureAwait(false); 
        }


        public void Dispose()
        {
            lock (options)
            {
                if (httpClient != null)
                {
                    httpClient.Dispose();
                    httpClient = null;
                }
            }
        }

    }
}
