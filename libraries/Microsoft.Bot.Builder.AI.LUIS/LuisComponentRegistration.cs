// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Define component assets for Luis.
    /// </summary>
    public class LuisComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        /// <summary>
        /// Gets a list of <see cref="LuisAdaptiveRecognizer"/> declarative type objects.
        /// </summary>
        /// <param name="resourceExplorer">An instance of <see cref="ResourceExplorer"/>.</param>
        /// <returns>A collection of <see cref="DeclarativeType"/> of <see cref="LuisAdaptiveRecognizer"/>.</returns>
        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            yield return new DeclarativeType<LuisAdaptiveRecognizer>(LuisAdaptiveRecognizer.Kind);
        }

        /// <summary>
        /// Gets a list of <see cref="LuisAdaptiveRecognizer"/> declarative type objects.
        /// </summary>
        /// <param name="resourceExplorer">An instance of <see cref="ResourceExplorer"/>.</param>
        /// <param name="sourceContext">An instance of <see cref="SourceContext"/>.</param>
        /// <returns>A collection of <see cref="DeclarativeType"/> of <see cref="LuisAdaptiveRecognizer"/>.</returns>
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield return new ArrayExpressionConverter<DynamicList>();
        }
    }
}
