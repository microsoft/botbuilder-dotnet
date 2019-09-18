// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers
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
