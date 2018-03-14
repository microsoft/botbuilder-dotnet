// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples.Echo
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the EchoBot.");

            var adapter = new ConsoleAdapter()
                .Use(new ConversationState<EchoState>(new MemoryStorage()));
            
            adapter.ProcessActivity(async (context) =>
            {
                var echoBot = new EchoBot(new MyService());

                await echoBot.OnReceiveActivity(context);
            }).Wait();
        }
    }
}
