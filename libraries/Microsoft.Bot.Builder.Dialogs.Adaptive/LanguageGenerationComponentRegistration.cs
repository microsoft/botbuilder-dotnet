// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class LanguageGenerationComponentRegistration : ComponentRegistration
    {
        public override IEnumerable<TypeRegistration> GetTypes()
        {
            yield return new TypeRegistration<TextTemplate>(TextTemplate.DeclarativeType);
            yield return new TypeRegistration<ActivityTemplate>(ActivityTemplate.DeclarativeType);
            yield return new TypeRegistration<StaticActivityTemplate>(StaticActivityTemplate.DeclarativeType);
        }

        public override IEnumerable<JsonConverter> GetConverters(ISourceMap sourceMap, IRefResolver refResolver, Stack<string> paths)
        {
            yield return new LanguageGeneratorConverter(refResolver, sourceMap, paths);
            yield return new ActivityTemplateConverter();
        }
    }
}
