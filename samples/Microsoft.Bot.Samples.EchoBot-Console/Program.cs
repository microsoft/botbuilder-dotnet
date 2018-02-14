// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

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
            Builder.Bot bot = new Builder.Bot(cc);
            bot.OnReceive(async (context) =>
                {
                    if (context.Request.Type == ActivityTypes.Message)
                    {
                        context.Reply($"echo: {context.Request.AsMessageActivity().Text}");
                    }
                });

            Console.WriteLine("Welcome to the EchoBot.");
            await cc.Listen();
        }        
    }
}
