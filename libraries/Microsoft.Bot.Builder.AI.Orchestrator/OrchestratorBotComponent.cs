﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.AI.Orchestrator
{
    /// <summary>
    /// Define component assets for Orchestrator.
    /// </summary>
    [Obsolete("The Bot Framework Orchestrator will be deprecated in the next version of the Bot Framework SDK.")]
    public class OrchestratorBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>(
                sp => new DeclarativeType<OrchestratorRecognizer>(OrchestratorRecognizer.Kind));
        }
    }
}
