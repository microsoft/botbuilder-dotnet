// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
