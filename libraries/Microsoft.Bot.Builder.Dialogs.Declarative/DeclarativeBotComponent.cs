// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    /// <summary>
    /// Registers declarative kinds.
    /// </summary>
    public class DeclarativeBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            services.AddSingleton<JsonConverterFactory, InterfaceConverterFactory<IStorage>>();
            services.AddSingleton<JsonConverterFactory, InterfaceConverterFactory<IRecognizer>>();
            services.AddSingleton<JsonConverterFactory, InterfaceConverterFactory<Dialog>>();
            services.AddSingleton<JsonConverterFactory, InterfaceConverterFactory<Recognizer>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ActivityConverter>>();
        }
    }
}
