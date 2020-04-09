// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers
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
