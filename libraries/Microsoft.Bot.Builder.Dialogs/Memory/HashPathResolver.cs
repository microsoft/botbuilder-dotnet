// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// Maps #xxx => turn.recognized.intents.xxx.
    /// </summary>
    public class HashPathResolver : AliasPathResolver
    {
        public HashPathResolver()
            : base(alias: "#", prefix: "turn.recognized.intents.")
        {
        }
    }
}
