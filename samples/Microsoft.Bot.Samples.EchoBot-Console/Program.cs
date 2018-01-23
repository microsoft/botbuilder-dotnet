using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples.EchoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var cc = new ConsoleAdapter();
            Builder.Bot bot = new Builder.Bot(cc)
                .OnReceive(async (context, next) =>
                {
                    if (context.Request.Type == ActivityTypes.Message)
                    {
                        context.Reply($"echo: {context.Request.AsMessageActivity().Text}");
                    }
                    await next();
                });

            Console.WriteLine("Welcome to the EchoBot.");
            await cc.Listen();
        }        
    }
}
