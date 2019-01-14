// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders
{
    public class IntentCommandDialogLoader : ComponentDialogLoader
    {
        public override object Load(JObject obj, JsonSerializer serializer, Type type)
        {
            return base.Load(obj, serializer, type);
        }
    }
}
