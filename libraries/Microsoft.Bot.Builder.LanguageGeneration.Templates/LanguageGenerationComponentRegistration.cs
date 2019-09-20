using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration.Generators;
using Microsoft.Bot.Builder.LanguageGeneration.Templates;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LanguageGenerationComponentRegistration : ComponentRegistration
    {
        public override IEnumerable<TypeRegistration> GetTypes()
        {
            yield return new TypeRegistration<TextTemplate>("Microsoft.TextTemplate");
            yield return new TypeRegistration<ActivityTemplate>("Microsoft.ActivityTemplate");
            yield return new TypeRegistration<StaticActivityTemplate>("Microsoft.StaticActivityTemplate");
        }

        public override IEnumerable<JsonConverter> GetConverters(Source.IRegistry registry, IRefResolver refResolver, Stack<string> paths)
        {
            yield return new LanguageGeneratorConverter(refResolver, registry, paths);
            yield return new ActivityTemplateConverter();
        }
    }
}
