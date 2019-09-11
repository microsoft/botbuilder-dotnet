// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// Maps @@ => turn.recognized.entitites.xxx array.
    /// </summary>
    public class AtAtPathResolver : AliasPathResolver
    {
        public AtAtPathResolver() 
            : base(alias: "@@", prefix: "turn.recognized.entities.")
        {
        }
    }
}
