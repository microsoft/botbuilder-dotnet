// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Loaders
{
    /// <summary>
    /// Type loader specifically for ComponentDialog since
    /// needs custom loading of the dialogs collection.
    /// </summary>
    public class ComponentDialogLoader : DefaultLoader
    {
        public override object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            ComponentDialog dialog = base.Load(obj, serializer, type) as ComponentDialog;

            // Dialogs are not a public property of ComponentDialog so we read the dialog
            // collection from the json and call AddDialog() on each dialog.
            if (dialog != null)
            {
                var dialogs = obj["dialogs"];

                // If there are dialogs, load them.
                if (dialogs != null)
                {
                    if (obj["dialogs"].Type != JTokenType.Array)
                    {
                        throw new JsonSerializationException("Expected array property \"dialogs\" in ComponentDialog");
                    }

                    foreach (var dialogJObj in dialogs)
                    {
                        var innerDialog = dialogJObj.ToObject<Dialog>(serializer);
                        dialog.AddDialog(innerDialog);
                    }
                }
            }

            return dialog; 
        }
    }
}
