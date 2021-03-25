// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests.TestComponents
{
    internal class TestBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            // Declarative type
            services.AddSingleton<DeclarativeType>(
                new DeclarativeType<TestDeclarativeType>(TestDeclarativeType.Kind));

            // Converter factory
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<TestDeclarativeConverter>>();
        }
    }
}
