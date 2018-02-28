using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Samples.EchoBot_AspNet461.Integration;
using System;
using System.Web.Http;

namespace Microsoft.Bot.Samples.EchoBot_AspNet461
{
    public class BotConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapBotFramework(botConfig =>
            {
                botConfig
                    //.UseApplicationIdentity("myApp123", "myAppPasswordXyz")
                    .UseMiddleware(new ConversationStateManagerMiddleware(new MemoryStorage()));
            });
        }
    }
}