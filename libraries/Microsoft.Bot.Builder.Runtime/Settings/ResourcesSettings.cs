// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    internal class ResourcesSettings
    {
        public string Storage { get; set; }

        public AdapterSettings[] Adapters { get; set; }
    }
}
