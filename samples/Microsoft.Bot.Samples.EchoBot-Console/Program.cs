// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Echo
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the EchoBot.");

            var adapter = new ConsoleAdapter();

            while (true)
            {
                var msg = Console.ReadLine();
                if (msg == null)
                    break;

                var activity = new Activity()
                {
                    Text = msg,
                    ChannelId = "console",
                    From = new ChannelAccount(id: "user", name: "User1"),
                    Recipient = new ChannelAccount(id: "bot", name: "Bot"),
                    Conversation = new ConversationAccount(id: "Convo1"),
                    Timestamp = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString(),
                    Type = ActivityTypes.Message
                };

                adapter.ProcessActivity(activity, async (context) =>
                {
                    var echoBot = new EchoBot();
                    await echoBot.OnReceiveActivity(context);
                }).Wait();
            }
        }
    }
}
