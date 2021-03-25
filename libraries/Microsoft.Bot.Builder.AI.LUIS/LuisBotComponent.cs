// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder.AI.LuisV3;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// LUIS <see cref="BotComponent"/> definition.
    /// </summary>
    public class LuisBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            // Converters
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ArrayExpressionConverter<DynamicList>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ArrayExpressionConverter<ExternalEntity>>>();

            // Declarative types
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<LuisAdaptiveRecognizer>(LuisAdaptiveRecognizer.Kind));
        }
    }
}
