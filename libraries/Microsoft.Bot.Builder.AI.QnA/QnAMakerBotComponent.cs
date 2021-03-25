// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.AI.QnA.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Class which contains registration of components for QnAMaker.
    /// </summary>
    public class QnAMakerBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<QnAMakerDialog>(QnAMakerDialog.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<QnAMakerRecognizer>(QnAMakerRecognizer.Kind));

            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ArrayExpressionConverter<Metadata>>>();
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ArrayExpressionConverter<QnARequestContext>>>();
        }
    }
}
