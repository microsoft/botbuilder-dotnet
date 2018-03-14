// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.WebApi;
using Microsoft.Bot.Builder.Core.Extensions;
using System.Configuration;
using System.Web.Http;

namespace Microsoft.Bot.Samples.Echo.AspNetWebApi
{
    public class BotConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapBotFramework(botConfig =>
            {
                botConfig
                    .UseMicrosoftApplicationIdentity(ConfigurationManager.AppSettings["BotFramework.MicrosoftApplicationId"], ConfigurationManager.AppSettings["BotFramework.MicrosoftApplicationPassword"])
                    .EnableProactiveMessages()
                    .UseMiddleware(new ConversationState<EchoState>(new MemoryStorage()));
            });
        }
    }
}