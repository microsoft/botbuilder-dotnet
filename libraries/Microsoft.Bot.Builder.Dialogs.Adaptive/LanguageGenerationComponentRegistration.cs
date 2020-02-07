// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.Versioning;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class LanguageGenerationComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public IEnumerable<DeclarativeType> GetDeclarativeTypes()
        {
            yield return new DeclarativeType<TextTemplate>(TextTemplate.DeclarativeType);
            yield return new DeclarativeType<ActivityTemplate>(ActivityTemplate.DeclarativeType);
            yield return new DeclarativeType<StaticActivityTemplate>(StaticActivityTemplate.DeclarativeType);
        }

        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, Stack<string> paths)
        {
            yield return new LanguageGeneratorConverter(resourceExplorer, paths);
            yield return new ActivityTemplateConverter();
        }
    }
}
