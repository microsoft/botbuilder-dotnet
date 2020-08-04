// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Orchestrator
{
    /// <summary>
    /// Define component assets for Luis.
    /// </summary>
    public class OrchestratorComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        /// <summary>
        /// Gets a list of <see cref="OrchestratorRecognizer"/> declarative type objects.
        /// </summary>
        /// <param name="resourceExplorer">An instance of <see cref="ResourceExplorer"/>.</param>
        /// <returns>A collection of <see cref="DeclarativeType"/> of <see cref="OrchestratorRecognizer"/>.</returns>
        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            yield return new DeclarativeType<OrchestratorRecognizer>(OrchestratorRecognizer.Kind);
        }

        /// <summary>
        /// Gets a list of <see cref="OrchestratorRecognizer"/> declarative type objects.
        /// </summary>
        /// <param name="resourceExplorer">An instance of <see cref="ResourceExplorer"/>.</param>
        /// <param name="sourceContext">An instance of <see cref="SourceContext"/>.</param>
        /// <returns>A collection of <see cref="DeclarativeType"/> of <see cref="OrchestratorRecognizer"/>.</returns>
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield break;
        }
    }
}
