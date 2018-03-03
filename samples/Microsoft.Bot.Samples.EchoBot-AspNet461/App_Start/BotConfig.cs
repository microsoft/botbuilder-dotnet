// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.WebApi;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using System.Web.Http;
using Microsoft.Bot.Samples.Echo;

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
                    .UseMiddleware(new ConversationState<EchoState>(new MemoryStorage()));
            });
        }
    }
}