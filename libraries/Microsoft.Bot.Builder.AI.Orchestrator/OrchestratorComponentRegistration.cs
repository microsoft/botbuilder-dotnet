// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Obsolete;

namespace Microsoft.Bot.Builder.AI.Orchestrator
{
    /// <summary>
    /// Define component assets for Orchestrator.
    /// </summary>
    [Obsolete("Use `OrchestratorBotComponent`.")]
    public class OrchestratorComponentRegistration : DeclarativeComponentRegistrationBridge<OrchestratorBotComponent>
    {
    }
}
