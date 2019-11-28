// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Skills
{
    internal class ChannelApiArgs
    {
        public string Method { get; set; }

        public object[] Args { get; set; }

        public object Result { get; set; }

        public Exception Exception { get; set; }
    }
}
