// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public class ComponentRegistration
    {
        public ComponentRegistration()
        {
        }

        public virtual IEnumerable<TypeRegistration> GetTypes()
        {
            yield break;
        }

        public virtual IEnumerable<JsonConverter> GetConverters(ISourceMap sourceMap, IRefResolver refResolver, Stack<string> paths)
        {
            yield break;
        }
    }
}
