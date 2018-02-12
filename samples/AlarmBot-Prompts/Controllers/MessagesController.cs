// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AlarmBot_Prompts
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        public static BotFrameworkAdapter activityAdapter = null;
        public static Bot bot = null;

        ///
        public MessagesController(IConfiguration configuration)
        {
            if (activityAdapter == null)
            {
                string applicationId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
                string applicationPassword = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value;

                // create the activity adapter that I will use to send/receive Activity objects with the user
                activityAdapter = new BotFrameworkAdapter(applicationId, applicationPassword);

                // pick your flavor of Key/Value storage
                //IStorage storage = new FileStorage(System.IO.Path.GetTempPath());
                IStorage storage = new MemoryStorage();
                //IStorage storage = new AzureTableStorage((System.Diagnostics.Debugger.IsAttached) ? "UseDevelopmentStorage=true;" : configuration.GetSection("DataConnectionString")?.Value, tableName: "AlarmBot");

                // create bot hooked up to the activity adapater
                bot = new Bot(activityAdapter)
                    .Use(new BotStateManager(storage)); // --- add Bot State Manager to automatically persist and load the context.State.Conversation and context.State.User objects
                bot.OnReceive(BotReceiveHandler);
            }
        }

        private async Task BotReceiveHandler(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                // Check for the triggering of a new topic
                var utterance = (context.Request.AsMessageActivity()?.Text ?? string.Empty).Trim().ToLowerInvariant();
                if (utterance.Contains("add alarm"))
                {
                    await new AddAlarm().Begin(context);
                }
                else if (utterance.Contains("delete alarm"))
                {
                    await new DeleteAlarm().Begin(context);
                }
                else if (utterance.Contains("show alarms"))
                {
                    await new ShowAlarms().Begin(context);
                }
                else if (utterance == "cancel")
                {
                    await new Cancel().Begin(context);
                }
                else
                {
                    // Continue the current topic
                    switch (context.State.Conversation["topic"])
                    {
                        case "addAlarm":
                            await new AddAlarm().RouteReply(context);
                            break;
                        case "deleteAlarm":
                            await new DeleteAlarm().RouteReply(context);
                            break;
                        default:
                            context.Reply(@"Hi! I'm a simple alarm bot. Say ""add alarm"", ""delete alarm"", or ""show alarms"".");
                            break;
                    }
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await activityAdapter.Receive(this.Request.Headers["Authorization"].FirstOrDefault(), activity);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }
    }
}
