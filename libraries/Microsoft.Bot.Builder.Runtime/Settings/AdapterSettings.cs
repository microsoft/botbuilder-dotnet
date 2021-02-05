﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    public class AdapterSettings
    {
        public string Name { get; set; }

        public string Route { get; set; }

        public bool Enabled { get; set; } = true;
    }
}
