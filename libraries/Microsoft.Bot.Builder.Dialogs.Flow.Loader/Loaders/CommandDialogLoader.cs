// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders
{
    public class SequenceDialogLoader : ILoader
    {
        public object Load(JObject obj, JsonSerializer serializer, Type type)
        {
            // TODO: ccastro to debug and remove this class. 
            // Temporary custom loader. To be removed. The DialogId is not getting 
            // to the hydrated object so adding custom logic here. 
            var result = obj.ToObject(type, serializer) as SequenceDialog;

            if (result != null)
            {
                //result.Dialog = obj["DialogId"].Value<string>();
            }

            return result;
        }
    }
}
