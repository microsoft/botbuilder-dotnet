// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Tests.Components.TestComponents;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Tests.Components.TestComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Runtime.Tests.Components.Implementations
{
    public class PirateBotComponent : BotComponent
    {
        public override void ConfigureServices(
            IServiceCollection services, 
            IConfiguration componentConfiguration)
        {
            // Component type
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<SendActivityAsPirate>(SendActivityAsPirate.Kind));

            // Custom converter with default constructor
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<SendActivityAsPirateConverter>>();

            // Custom memory scope
            services.AddSingleton<MemoryScope, TestMemoryScope>();

            // Custom path resolver
            services.AddSingleton<IPathResolver, DoubleCaratPathResolver>();

            // Custom http adapter
            services.AddSingleton<IBotFrameworkHttpAdapter, ContosoAdapter>();

            // Custom converter with custom constructor receiving resource explorer and source context (advanced)
            services.AddSingleton<JsonConverterFactory>(
                sp => new LambdaJsonConverterFactory((r, s) => new SendActivityAsPirateConverter(r, s)));
        }
    }
}
