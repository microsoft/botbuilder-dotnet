// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers
{
    /// <summary>
    /// Resolve $xxx.
    /// </summary>
    /// <remarks>
    /// $xxx -> dialog.x.
    /// </remarks>
    public class DollarPathResolver : AliasPathResolver
    {
        public DollarPathResolver()
            : base(alias: "$", prefix: "dialog.")
        {
        }
    }
}
