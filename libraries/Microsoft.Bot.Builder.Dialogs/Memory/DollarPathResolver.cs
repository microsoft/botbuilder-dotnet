// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// Resolve $xxx.
    /// </summary>
    /// <remarks>
    /// $xxx -> parent.x 
    /// </remarks>
    public class DollarPathResolver : AliasPathResolver
    {
        public DollarPathResolver()
            : base(alias: "$", prefix: "dialog.")
        {
        }
    }
}
