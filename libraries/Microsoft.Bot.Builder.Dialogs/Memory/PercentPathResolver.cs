// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// Maps %xxx => dialog.options.xxx
    /// </summary>
    public class PercentPathResolver : AliasPathResolver
    {
        public PercentPathResolver()
            : base(alias: "%", prefix: "dialog.options.")
        {
        }
    }
}
