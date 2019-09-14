using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public class DialogComponentRegistration : ComponentRegistration
    {
        public override IEnumerable<JsonConverter> GetConverters(Source.IRegistry registry, IRefResolver refResolver, Stack<string> paths)
        {
            yield return new InterfaceConverter<Dialog>(refResolver, registry, paths);
            yield return new InterfaceConverter<IStorage>(refResolver, registry, paths);
            yield return new InterfaceConverter<IRecognizer>(refResolver, registry, paths);
            yield return new ExpressionConverter();
            yield return new ActivityConverter();
        }
    }
}
