// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// ComponentRegistration class for language generation resources.
    /// </summary>
    public class LanguageGenerationComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        /// <summary>
        /// Return declarative types for Language Generation.
        /// </summary>
        /// <returns>DeclarativeTypes enumeration.</returns>
        public IEnumerable<DeclarativeType> GetDeclarativeTypes()
        {
            yield return new DeclarativeType<TextTemplate>(TextTemplate.DeclarativeType);
            yield return new DeclarativeType<ActivityTemplate>(ActivityTemplate.DeclarativeType);
            yield return new DeclarativeType<StaticActivityTemplate>(StaticActivityTemplate.DeclarativeType);
        }

        /// <summary>
        /// Return JsonConverters for LanguageGeneration resources.
        /// </summary>
        /// <param name="resourceExplorer">resource explorer to use for resolving references.</param>
        /// <param name="paths">contextual path stack to use to build debugger.sourcemap.</param>
        /// <returns>enumeration of jsonconverters.</returns>
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, Stack<string> paths)
        {
            yield return new LanguageGeneratorConverter(resourceExplorer, paths);
            yield return new ActivityTemplateConverter();
        }
    }
}
