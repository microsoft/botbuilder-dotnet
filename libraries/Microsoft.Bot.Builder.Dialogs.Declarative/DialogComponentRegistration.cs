using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public class DialogComponentRegistration : ComponentRegistration
    {
        public override IEnumerable<JsonConverter> GetConverters(ISourceMap sourceMap, IRefResolver refResolver, Stack<string> paths)
        {
            yield return new InterfaceConverter<Dialog>(refResolver, sourceMap, paths);
            yield return new InterfaceConverter<IStorage>(refResolver, sourceMap, paths);
            yield return new InterfaceConverter<IRecognizer>(refResolver, sourceMap, paths);
            yield return new ExpressionConverter();
            yield return new ActivityConverter();
        }
    }
}
