// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Types
{
    public class UriTypeBinder : DefaultSerializationBinder
    {
        private ResourceExplorer resourceExplorer;

        public UriTypeBinder(ResourceExplorer resourceExplorer)
        {
            this.resourceExplorer = resourceExplorer;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = resourceExplorer.GetKindForType(serializedType).ToString();

            if (string.IsNullOrEmpty(typeName))
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
            }
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            var type = resourceExplorer.GetTypeForKind(typeName);

            if (type != default(Type))
            {
                return type;
            }

            return base.BindToType(assemblyName, typeName);
        }
    }
}
