// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Servers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.EchoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static Task MainAsync(string[] args)
        {
            Console.WriteLine("Welcome to the EchoBot.");
            var botServer = new ConsoleBotServer();
            return botServer.ProcessActivity(async (context) =>
            {
                if (context.Request.Type == ActivityTypes.Message)
                {
                    context.Reply($"echo: {context.Request.AsMessageActivity().Text}");
                }
            });
        }
    }
}
