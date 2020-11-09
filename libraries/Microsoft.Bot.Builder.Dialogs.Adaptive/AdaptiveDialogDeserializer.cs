// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Internal serializer for DynamicBeginDialog which binds the x.dialog resourceId to the DyanmicBeginDialog.Dialog property.
    /// </summary>
    internal class AdaptiveDialogDeserializer : ICustomDeserializer
    {
        private readonly ResourceExplorer resourceExplorer;

        public AdaptiveDialogDeserializer(ResourceExplorer resourceExplorer)
        {
            this.resourceExplorer = resourceExplorer;
        }

        public object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            var dialog = obj.ToObject<AdaptiveDialog>(serializer);

            var refDialogs = dialog.ReferecedDialogs;
            if (refDialogs != null && refDialogs.Count > 0)
            {
                foreach (var refDialog in refDialogs)
                {
                    var resourceID = resourceExplorer.GetResource($"{refDialog}.dialog");
                    var actualDialog = resourceExplorer.LoadType<Dialog>(resourceID);
                    dialog.Dialogs.Add(actualDialog);
                }
            }

            return dialog;
        }
    }
}
