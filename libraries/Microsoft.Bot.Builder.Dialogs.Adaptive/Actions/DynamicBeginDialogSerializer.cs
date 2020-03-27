// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Internal serializer for DynamicBeginDialog which binds the x.dialog resourceId to the DyanmicBeginDialog.Dialog property.
    /// </summary>
    internal class DynamicBeginDialogSerializer : ICustomDeserializer
    {
        private ResourceExplorer resourceExplorer;
        private string resourceId;

        public DynamicBeginDialogSerializer(ResourceExplorer resourceExplorer, string resourceId)
        {
            this.resourceExplorer = resourceExplorer;
            this.resourceId = resourceId;
        }

        public object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            var schemaDialog = obj.ToObject<DynamicBeginDialog>(serializer);
            
            // load dialog from the resource.
            schemaDialog.Dialog = resourceExplorer.LoadType<Dialog>(this.resourceId);
            return schemaDialog;
        }
    }
}
