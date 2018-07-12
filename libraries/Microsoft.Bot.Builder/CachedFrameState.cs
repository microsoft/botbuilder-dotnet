// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    public class CachedFrameState
    {
        public object State { get; set; }

        public string Hash { get; set; }

        public bool Accessed { get; set; }
    }
}
