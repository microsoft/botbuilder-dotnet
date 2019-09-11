// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
