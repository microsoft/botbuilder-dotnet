using Microsoft.Bot.Samples.Middleware;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Conversation;
using Microsoft.Bot.Builder.Storage;

namespace Microsoft.Bot.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var cc = new Microsoft.Bot.Builder.Adapters.ConsoleAdapter();
          
            Builder.Bot bot = new Builder.Bot(cc)
                .Use(new MemoryStorage())
                .Use(new BotStateManager())
                .Use(CreateRegEx())
                .Use(new EchoMiddleware())
                .Use(new ReverseMiddleWare())
                .Use(new ProactiveMiddleware())
                //.Use(new ActivityRoutingMiddleware(Routing.BuildHelpRouting()))
                //.Use(new ActivityRoutingMiddleware(Routing.BuildLoggingRouting()))                
                .Use(new ConsoleLogger());

            await cc.Listen();
        }

        public static RegExpRecognizerMiddleware CreateRegEx()
        {
            RegExpRecognizerMiddleware regExpMiddleware = new RegExpRecognizerMiddleware();
            regExpMiddleware.AddIntent(
                "echoIntent", new Regex("echo (?<WhatToEcho>.*)", RegexOptions.IgnoreCase));

            regExpMiddleware.AddIntent(
                "delayIntent", new Regex("delay (?<howLong>.*)", RegexOptions.IgnoreCase));

            regExpMiddleware.AddIntent(
                "reverseIntent", new Regex("reverse", RegexOptions.IgnoreCase));

            regExpMiddleware.AddIntent(
                "help", new Regex("help", RegexOptions.IgnoreCase));

            regExpMiddleware.AddIntent(
                "logging", new Regex("logging", RegexOptions.IgnoreCase));

            return regExpMiddleware;
        }
    }
}
