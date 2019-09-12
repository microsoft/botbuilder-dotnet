// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Form;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Loaders
{
    public class FormDialogLoader : ICustomDeserializer
    {
        public FormDialogLoader()
        {
        }

        public object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            FormDialog form;
            if (obj["schema"]?.Type == JTokenType.String)
            {
                var schema = DialogSchema.ReadSchema(obj["schema"].Value<string>());
                form = new FormDialog(schema);
            }
            else
            {
                form = obj.ToObject<FormDialog>(serializer);
            }

            return form;
        }
    }
}
