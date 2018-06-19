// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Samples.Echo
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the EchoBot.");

            var adapter = new ConsoleAdapter();                            
            adapter.ProcessActivity(async (context) =>
            {
                var echoBot = new EchoBot();
                await echoBot.OnTurn(context);
            }).Wait();
        }
    }
}
