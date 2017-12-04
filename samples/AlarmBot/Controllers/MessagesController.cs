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
                activityAdapter = new BotFrameworkAdapter(configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value,
                                                              configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value);

                bot = new Bot(activityAdapter)
                    .Use(new MemoryStorage())
                    .Use(new BotStateManager())
                    .UseTemplates(DefaultTopic.ReplyTemplates)
                    .UseTemplates(ShowAlarmsTopic.ReplyTemplates)
                    .UseTemplates(AddAlarmTopic.ReplyTemplates)
                    .UseTemplates(DeleteAlarmTopic.ReplyTemplates)
                    .Use(new RegExpRecognizerMiddleware()
                        .AddIntent("showAlarms", new Regex("show alarms(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("addAlarm", new Regex("add alarm(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("deleteAlarm", new Regex("delete alarm(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("help", new Regex("help(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("cancel", new Regex("cancel(.*)", RegexOptions.IgnoreCase)))
                    .OnReceive(async (context) =>
                    {
                        bool handled = false;
                        var activeTopic = context.State.Conversation[ConversationProperties.ACTIVETOPIC] as ITopic;

                        // start with default topic
                        if (activeTopic == null)
                        {
                            activeTopic = new DefaultTopic();
                            context.State.Conversation[ConversationProperties.ACTIVETOPIC] = activeTopic;
                            handled = await activeTopic.StartTopic(context);
                        }
                        else
                        {
                            // route to active topic
                            handled = await activeTopic.ContinueTopic(context);

                            // AlarmBot only needs to transition from defaultTopic -> subTopic and back, so 
                            // if result is false and not default topic, then switch back to default topic
                            if (handled == false && !(activeTopic is DefaultTopic))
                            {
                                // resume default topic
                                activeTopic = new DefaultTopic(); 
                                context.State.Conversation[ConversationProperties.ACTIVETOPIC] = activeTopic;
                                handled = await activeTopic.ResumeTopic(context);
                            }
                        }
                        return new ReceiveResponse(handled);
                    });
            }
        }


        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async void Post([FromBody]Activity activity)
        {
            await activityAdapter.Receive(HttpContext.Request.Headers, activity);
        }
    }

}
