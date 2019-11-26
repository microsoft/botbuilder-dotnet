// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    public abstract class TestAction
    {
        public abstract Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback);

        protected void RegisterSourcePath(string path, int line)
        {
            if (!string.IsNullOrEmpty(path))
            {
                DebugSupport.SourceMap.Add(this, new SourceRange() { Path = path, StartPoint = new SourcePoint() { LineIndex = line, CharIndex = 0 }, EndPoint = new SourcePoint() { LineIndex = line + 1, CharIndex = 0 }, });
            }
        }
    }
}
