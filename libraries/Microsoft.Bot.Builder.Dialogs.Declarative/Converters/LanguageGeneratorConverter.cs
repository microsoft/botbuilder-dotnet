// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    public class LanguageGeneratorConverter : InterfaceConverter<ILanguageGenerator>
    {
        public LanguageGeneratorConverter(IRefResolver refResolver, Source.IRegistry registry, Stack<string> paths)
            : base(refResolver, registry, paths)
        {
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                string readerValue = reader.Value.ToString();
                return TypeFactory.Build<ILanguageGenerator>("DefaultLanguageGenerator", readerValue, serializer);
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
