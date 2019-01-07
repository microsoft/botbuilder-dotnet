using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders
{
    public class DefaultLoader : ILoader
    {
        public object Load(JObject obj, JsonSerializer serializer, Type type)
        {
            return obj.ToObject(type, serializer);
        }
    }
}
