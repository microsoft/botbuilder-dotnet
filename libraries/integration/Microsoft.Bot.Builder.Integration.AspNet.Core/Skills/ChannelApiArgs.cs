// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Skills
{
    internal class ChannelApiArgs
    {
        public ChannelApiMethods Methods { get; set; }

        public object[] Args { get; set; }

        public object Result { get; set; }
    }
}
