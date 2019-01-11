using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Contract;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Converters;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders
{
    public class CommandDialogLoader : ILoader
    {
        public object Load(JObject obj, JsonSerializer serializer, Type type)
        {
            // TODO: ccastro to debug and remove this class. 
            // Temporary custom loader. To be removed. The DialogId is not getting 
            // to the hydrated object so adding custom logic here. 
            var result = obj.ToObject(type, serializer) as CommandDialog;

            if (result != null)
            {
                result.DialogId = obj["DialogId"].Value<string>();
            }

            return result;
        }
    }
}
