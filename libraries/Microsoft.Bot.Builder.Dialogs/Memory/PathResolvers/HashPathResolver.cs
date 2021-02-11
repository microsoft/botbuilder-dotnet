// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers
{
    /// <summary>
    /// Maps #xxx => turn.recognized.intents.xxx.
    /// </summary>
    public class HashPathResolver : AliasPathResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HashPathResolver"/> class.
        /// </summary>
        public HashPathResolver()
            : base(alias: "#", prefix: "turn.recognized.intents.")
        {
        }
    }
}
