// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Tests.Components.TestComponents
{
    /// <summary>
    /// Resolve ^^xxx.
    /// </summary>
    /// <remarks>
    /// ^^xxx -> test.x.
    /// </remarks>
    public class DoubleCaratPathResolver : AliasPathResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleCaratPathResolver"/> class.
        /// </summary>
        public DoubleCaratPathResolver()
            : base(alias: "^^", prefix: "test.")
        {
        }
    }
}
