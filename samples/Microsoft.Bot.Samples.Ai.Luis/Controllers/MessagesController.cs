
// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Schema;
using Microsoft.Cognitive.LUIS;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.Ai.Luis.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        static BotFrameworkAdapter adapter;
    
        public MessagesController(IConfiguration configuration)
        {
            if (adapter == null)
            {
                var luisModel = new LuisModel("modelId", "subscriptionKey", new Uri("https://RegionOfYourLuisApp.api.cognitive.microsoft.com/luis/v2.0/apps/"));
                var options = new LuisRequest { Verbose = true }; // If you want to get all intents scorings, add verbose in luisOptions
                //LuisRequest options = null;
                
                adapter = new BotFrameworkAdapter(new ConfigurationCredentialProvider(configuration))
                    .Use(new LuisRecognizerMiddleware(luisModel, luisOptions: options));
            }
        }

        private Task BotReceiveHandler(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                var luisResult = context.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);

                if (luisResult != null)
                {
                    (string key, double score) topItem = luisResult.GetTopScoringIntent();
                    context.SendActivity($"The **top intent** was: **'{topItem.key}'**, with score **{topItem.score}**");

                    context.SendActivity($"Detail of intents scorings:");
                    var intentsResult = new List<string>();
                    foreach (var intent in luisResult.Intents)
                    {
                        intentsResult.Add($"* '{intent.Key}', score {intent.Value}");
                    }
                    context.SendActivity(string.Join("\n\n", intentsResult));
                }
            }
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await adapter.ProcessActivity(this.Request.Headers["Authorization"].FirstOrDefault(), activity, BotReceiveHandler);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }
    }
}
