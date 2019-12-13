// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Mock Client to access a QnA Maker knowledge base.
    /// </summary>
    public class MockQnAMakerClient : IQnAMakerClient
    {
        public Task CallTrainAsync(FeedbackRecords feedbackRecords)
        {
            return Task.FromResult<object>(null);
        }

        public Task<QueryResult[]> GetAnswersAsync(ITurnContext turnContext, QnAMakerOptions options, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null)
        {
            throw new NotImplementedException();
        }

        public Task<QueryResults> GetAnswersRawAsync(ITurnContext turnContext, QnAMakerOptions options, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var query = turnContext.Activity.Text;

            var answers = new QueryResult[] { };

            var result = new QueryResults
            {
                ActiveLearningEnabled = true,
                Answers = answers
            };

            if (query.Equals("Q11"))
            {
                answers = new QueryResult[]
                {
                    new QueryResult { Answer = "A1", Context = null, Id = 15, Score = 0.80F, Questions = new string[] { "Q1" }, Source = "Editorial" },
                    new QueryResult { Answer = "A2", Context = null, Id = 16, Score = 0.78F, Questions = new string[] { "Q2" }, Source = "Editorial" },
                    new QueryResult { Answer = "A3", Context = null, Id = 17, Score = 0.75F, Questions = new string[] { "Q3" }, Source = "Editorial" },
                    new QueryResult { Answer = "A4", Context = null, Id = 18, Score = 0.50F, Questions = new string[] { "Q4" }, Source = "Editorial" }
                };
                result.Answers = answers;
            }

            // Output for question only ranker.
            if (query.Equals("What ranker do you want to use?") && options.RankerType.Equals(RankerTypes.QuestionOnly))
            {
                answers = new QueryResult[]
                {
                    new QueryResult { Answer = "We are using QuestionOnly ranker.", Context = null, Id = 25, Score = 0.80F, Questions = new string[] { "Question only ranker" }, Source = "Editorial" },
                };
                result.Answers = answers;
            }

            // Output for question only ranker.
            if (query.Equals("Surface book 2 price") && options.IsTest)
            {
                answers = new QueryResult[]
                {
                    new QueryResult { Answer = "Surface book 2 price is $1400.", Context = null, Id = 26, Score = 0.80F, Questions = new string[] { "Price range for surface laptop" }, Source = "Editorial" },
                };
                result.Answers = answers;
            }

            return Task.FromResult(result);
        }

        public QueryResult[] GetLowScoreVariation(QueryResult[] queryResult)
        {
            if (queryResult.Length > 1)
            {
                return new QueryResult[]
                   {
                       new QueryResult { Answer = "A1", Context = null, Id = 15, Score = 80, Questions = new string[] { "Q1" }, Source = "Editorial" },
                       new QueryResult { Answer = "A2", Context = null, Id = 16, Score = 78, Questions = new string[] { "Q2" }, Source = "Editorial" },
                       new QueryResult { Answer = "A3", Context = null, Id = 17, Score = 75, Questions = new string[] { "Q3" }, Source = "Editorial" }
                   };
            }
            else
            {
                return queryResult;
            }
        }
    }
}
