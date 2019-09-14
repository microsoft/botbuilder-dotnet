using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.LanguageGeneration.Generators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration.Templates
{
    public class LanguageGeneratorLoader : ICustomDeserializer
    {
        public object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            return new ResourceMultiLanguageGenerator(obj.Value<string>());
        }
    }
}
