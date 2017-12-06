using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Storage;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using AlarmBot.Topics;
using AlarmBot.Models;
using Microsoft.Bot.Builder.Templates;

namespace AlarmBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        public static BotFrameworkAdapter activityAdapter = null;
        public static Bot bot = null;

        public MessagesController(IConfiguration configuration)
        {
            if (activityAdapter == null)
            {
                // create the activity adapter that I will use to send/receive Activity objects with the user
                activityAdapter = new BotFrameworkAdapter(configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value,
                                                              configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value);

                // create bot hooked up to the activity adapater
                bot = new Bot(activityAdapter)
                    // --- register AzureTableStorage as the IStorage key/object system for any component to store objects
                    .Use(new AzureTableStorage(
                        // if debugger is attached, use local storage emulator, otherwise use real table
                        (System.Diagnostics.Debugger.IsAttached) ? "UseDevelopmentStorage=true;" : configuration.GetSection("DataConnectionString")?.Value,
                        tableName: "AlarmBot"))
                    
                    // --- add Bot State Manager to automatically persist and load the context.State.Conversation and context.State.User objects
                    .Use(new BotStateManager()) 

                    // --- register reply templates dictionaries for all the components using .ReplyWith() 
                    .UseTemplates(DefaultTopic.ReplyTemplates)
                    .UseTemplates(ShowAlarmsTopic.ReplyTemplates)
                    .UseTemplates(AddAlarmTopic.ReplyTemplates)
                    .UseTemplates(DeleteAlarmTopic.ReplyTemplates)

                    // --- register intent recognizers, 
                    // These inspect context.Request.Text and will set context.TopIntent based on regular expression based patterns
                    .Use(new RegExpRecognizerMiddleware()
                        .AddIntent("showAlarms", new Regex("show alarms(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("addAlarm", new Regex("add alarm(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("deleteAlarm", new Regex("delete alarm(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("help", new Regex("help(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("cancel", new Regex("cancel(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("confirmYes", new Regex("(y|yes|yep)\\w+", RegexOptions.IgnoreCase))
                        .AddIntent("confirmNo", new Regex("(n|no|nope)\\w+", RegexOptions.IgnoreCase)))

                    // --- Bot logic 
                    .OnReceive(async (context) =>
                    {
                        bool handled = false;
                        // Get the current ActiveTopic from my conversation state
                        var activeTopic = context.State.Conversation[ConversationProperties.ACTIVETOPIC] as ITopic;

                        // if there isn't one 
                        if (activeTopic == null)
                        {
                            // use default topic
                            activeTopic = new DefaultTopic();
                            context.State.Conversation[ConversationProperties.ACTIVETOPIC] = activeTopic;
                            handled = await activeTopic.StartTopic(context);
                        }
                        else
                        {
                            // continue to use the active topic
                            handled = await activeTopic.ContinueTopic(context);
                        }

                        // AlarmBot only needs to transition from defaultTopic -> subTopic and back, so 
                        // if activeTopic's result is false and the activeToic is NOT the default topic, we switch back to default topic
                        if (handled == false && !(context.State.Conversation[ConversationProperties.ACTIVETOPIC] is DefaultTopic))
                        {
                            // resume default topic
                            activeTopic = new DefaultTopic();
                            context.State.Conversation[ConversationProperties.ACTIVETOPIC] = activeTopic;
                            handled = await activeTopic.ResumeTopic(context);
                        }
                    });
            }
        }


        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async void Post([FromBody]Activity activity)
        {
            // We have an activity from the user, give it to the activityAdapter/Bot to process it
            await activityAdapter.Receive(HttpContext.Request.Headers, activity);
        }
    }

}
