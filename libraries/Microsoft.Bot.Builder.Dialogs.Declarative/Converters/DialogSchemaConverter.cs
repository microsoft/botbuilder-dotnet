// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Parsers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Dialogs.Form;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    public class DialogSchemaConverter : InterfaceConverter<DialogSchema>
    {

        public DialogSchemaConverter(IRefResolver refResolver, Source.IRegistry registry, Stack<string> paths)
            : base(refResolver, registry, paths)
        { }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            DialogSchema schema;
            if (reader.ValueType == typeof(string))
            {
                // TODO: Need to dref string with resolver
                schema = new DialogSchema(reader.Value as JObject);
            }
            else
            {
                schema = new DialogSchema(reader.Value as JObject);
            }

            return schema;
        }
    }
}

