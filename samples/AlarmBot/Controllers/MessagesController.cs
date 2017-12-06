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
using AlarmBot.TopicViews;

namespace AlarmBot.Controllers
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
                // create the activity adapter that I will use to send/receive Activity objects with the user
                activityAdapter = new BotFrameworkAdapter(configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value,
                                                              configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value);

                // create bot hooked up to the activity adapater
                bot = new Bot(activityAdapter)
                    .Use(new AzureTableStorage((System.Diagnostics.Debugger.IsAttached) ? "UseDevelopmentStorage=true;" : configuration.GetSection("DataConnectionString")?.Value,
                        tableName: "AlarmBot")) 
                    .Use(new BotStateManager()) // --- add Bot State Manager to automatically persist and load the context.State.Conversation and context.State.User objects
                    .Use(new DefaultTopicView())
                    .Use(new ShowAlarmsTopicView())
                    .Use(new AddAlarmTopicView())
                    .Use(new DeleteAlarmTopicView())
                    .Use(new RegExpRecognizerMiddleware()
                        .AddIntent("showAlarms", new Regex("show alarms(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("addAlarm", new Regex("add alarm(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("deleteAlarm", new Regex("delete alarm(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("help", new Regex("help(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("cancel", new Regex("cancel(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("confirmYes", new Regex("(yes|yep)\\w+", RegexOptions.IgnoreCase))
                        .AddIntent("confirmNo", new Regex("(no|nope)\\w+", RegexOptions.IgnoreCase)))
                    .OnReceive(async (context) =>
                    {
                        // --- Bot logic 
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
