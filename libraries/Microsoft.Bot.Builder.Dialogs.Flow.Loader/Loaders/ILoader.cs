// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Loaders
{
    public interface ILoader
    {
        object Load(JObject obj, JsonSerializer serializer, Type type);
    }
}
