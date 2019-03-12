// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Web.Http;

namespace Microsoft.Bot.Builder.TestBot.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(config =>
            {
                BotConfig.Register(config);
            });
        }
    }
}
