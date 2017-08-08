using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Samples.Connector.EchoBot;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Samples.ConsoleConnector
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            // setup service collection with echo middleware
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IMiddleware, EchoMiddleWare>();
            serviceCollection.UseBotServices().UseConsoleConnector();
            
            await Builder.ConsoleConnector.Listen(serviceCollection);
        }
    }
}
