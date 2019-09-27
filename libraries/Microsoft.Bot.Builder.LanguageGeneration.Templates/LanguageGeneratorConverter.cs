// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration.Generators;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LanguageGeneratorConverter : InterfaceConverter<ILanguageGenerator>
    {
        public LanguageGeneratorConverter(IRefResolver refResolver, ISourceMap sourceMap, Stack<string> paths)
            : base(refResolver, sourceMap, paths)
        {
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                string readerValue = reader.Value.ToString();

                return new ResourceMultiLanguageGenerator(readerValue);
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
