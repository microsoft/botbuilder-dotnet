// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotKit.Adapters.Slack
{
    public class AdapterError
    {
        public AdapterError()
        {
        }

        public string Name { get; private set; }

        public string Error { get; private set; }
    }
}
