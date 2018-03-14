
// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.LUIS;
using Microsoft.Bot.Schema;
using Microsoft.Cognitive.LUIS;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                adapter = new BotFrameworkAdapter(new ConfigurationCredentialProvider(configuration))
                    .Use(new BatchOutputMiddleware())
                    .Use(new LuisRecognizerMiddleware(new LuisModel("modelId", "subscriptionKey", new Uri("https://xxxxxx.api.cognitive.microsoft.com/luis/v2.0/apps/"))
                    // If you want to get all intents scorings, add verbose in luisOptions
                    // .Use(new LuisRecognizerMiddleware(new LuisModel("modelId", "subscriptionKey", new Uri("https://xxxxxx.api.cognitive.microsoft.com/luis/v2.0/apps/")), null, luisOptions: new LuisRequest { Verbose = true }
                    ));
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
                    context.Batch().Reply($"The **top intent** was: **'{topItem.key}'**, with score **{topItem.score}**");

                    context.Batch().Reply($"Detail of intents scorings:");
                    var intentsResult = new List<string>();
                    foreach (var intent in luisResult.Intents)
                    {
                        intentsResult.Add($"* '{intent.Key}', score {intent.Value}");
                    }
                    context.Batch().Reply(string.Join("\n\n", intentsResult));
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
