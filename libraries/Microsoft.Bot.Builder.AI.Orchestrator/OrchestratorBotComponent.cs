// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.AI.Orchestrator
{
    /// <summary>
    /// Define component assets for Orchestrator.
    /// </summary>
    public class OrchestratorBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            services.AddSingleton<DeclarativeType>(
                sp => new DeclarativeType<OrchestratorAdaptiveRecognizer>(OrchestratorAdaptiveRecognizer.Kind));
        }
    }
}
