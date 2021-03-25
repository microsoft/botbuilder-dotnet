// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Tests.Components.TestComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Runtime.Tests.Components.Implementations
{
    public class AdventureWorksAdapterComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            var options = configuration.Get<AdventureWorksAdapterOptions>();

            if (options != null)
            {
                services.AddSingleton<IBotFrameworkHttpAdapter>(new AdventureWorksAdapter(options));
            }
        }
    }
}
