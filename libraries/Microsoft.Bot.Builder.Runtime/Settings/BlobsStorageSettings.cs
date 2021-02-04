// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    internal class BlobsStorageSettings
    {
        public string ConnectionString { get; set; }

        public string ContainerName { get; set; }

        public bool IsConfigured()
        {
            return !(string.IsNullOrEmpty(ConnectionString) || string.IsNullOrEmpty(ContainerName));
        }
    }
}
