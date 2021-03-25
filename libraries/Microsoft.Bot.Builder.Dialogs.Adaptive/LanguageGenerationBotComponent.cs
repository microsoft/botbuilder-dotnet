// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// ComponentRegistration class for language generation resources.
    /// </summary>
    public class LanguageGenerationBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<TextTemplate>(TextTemplate.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ActivityTemplate>(ActivityTemplate.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<StaticActivityTemplate>(StaticActivityTemplate.Kind));

            services.AddSingleton<JsonConverterFactory>(
                sp => new LambdaJsonConverterFactory((r, s) => new LanguageGeneratorConverter(r, s)));

            services.AddSingleton<JsonConverterFactory>(
                sp => new LambdaJsonConverterFactory((r, s) => new ITemplateActivityConverter(r, s)));
        }
    }
}
