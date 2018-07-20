// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS.Models;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Standard implementation of ILuisService against actual LUIS service.
    /// </summary>
    [Serializable]
    public sealed class LuisService : ILuisService
    {
        private static readonly HttpClient DefaultHttpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) };
        private readonly ILuisModel model;

        private HttpClient _httpClient = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisService"/> class using the model information.
        /// </summary>
        /// <param name="model">The LUIS model information.</param>
        /// <param name="httpClient">an optional alternate HttpClient.</param>
        public LuisService(ILuisModel model, HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? DefaultHttpClient;
            SetField.NotNull(out this.model, nameof(model), model);
        }

        /// <summary>
        /// Updates the <see cref="LuisResult.Intents"/> to be backward compatible with
        /// previous versions.
        /// </summary>
        /// <param name="result">The LUIS result to update.</param>
        public static void Fix(LuisResult result)
        {
            // fix up Luis result for backward compatibility
            // v2 api is not returning list of intents if verbose query parameter
            // is not set. This will move IntentRecommendation in TopScoringIntent
            // to list of Intents.
            if (result.Intents == null || result.Intents.Count == 0)
            {
                if (result.TopScoringIntent != null)
                {
                    result.Intents = new List<IntentRecommendation> { result.TopScoringIntent };
                }
            }
        }

        /// <summary>
        /// Using this service's <see cref="ILuisModel"/>, modifies a LUIS request to specify
        /// query parameters like spelling or logging.
        /// </summary>
        /// <param name="request">The request to modify.</param>
        /// <returns>The modified request.</returns>
        public LuisRequest ModifyRequest(LuisRequest request) => model.ModifyRequest(request);

        Uri ILuisService.BuildUri(LuisRequest luisRequest) => luisRequest.BuildUri(this.model);

        /// <summary>
        /// Using this service's <see cref="ILuisModel"/>.<see cref="ILuisModel.Threshold"/>,
        /// updates a result's <see cref="LuisResult.TopScoringIntent"/>.
        /// </summary>
        /// <param name="result">The LUIS result to update.</param>
        public void ApplyThreshold(LuisResult result)
        {
            if (result.TopScoringIntent.Score > model.Threshold)
            {
                return;
            }

            result.TopScoringIntent.Intent = "None";
            result.TopScoringIntent.Score = 1.0d;
        }

        async Task<LuisResult> ILuisService.QueryAsync(Uri uri, CancellationToken token)
        {
            string json;
            using (var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            try
            {
                var result = JsonConvert.DeserializeObject<LuisResult>(json);
                Fix(result);
                ApplyThreshold(result);
                return result;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Unable to deserialize the LUIS response.", ex);
            }
        }
    }
}
