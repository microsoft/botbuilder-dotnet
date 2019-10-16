// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Skills.Preview
{
    internal class ChannelApiArgs
    {
        public string Method { get; set; }

        public object[] Args { get; set; }

        public object Result { get; set; }
    }
}
