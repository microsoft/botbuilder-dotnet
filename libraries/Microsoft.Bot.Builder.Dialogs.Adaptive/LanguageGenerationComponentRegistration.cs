using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.Luis;
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
            yield return new TypeRegistration<TextTemplate>("Microsoft.TextTemplate");
            yield return new TypeRegistration<ActivityTemplate>("Microsoft.ActivityTemplate");
            yield return new TypeRegistration<StaticActivityTemplate>("Microsoft.StaticActivityTemplate");
        }

        public override IEnumerable<JsonConverter> GetConverters(ISourceMap sourceMap, IRefResolver refResolver, Stack<string> paths)
        {
            yield return new LanguageGeneratorConverter(refResolver, sourceMap, paths);
            yield return new ActivityTemplateConverter();
        }
    }
}
