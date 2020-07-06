// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Debugging;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.UserTokenMocks
{
    /// <summary>
    /// Base class for all user token mocks.
    /// </summary>
    public abstract class UserTokenMock
    {
        /// <summary>
        /// This function will be called to setup the test adapter.
        /// </summary>
        /// <param name="adapter">The target test adapter.</param>
        public abstract void Setup(TestAdapter adapter);

        protected void RegisterSourcePath(string path, int line)
        {
            if (!string.IsNullOrEmpty(path))
            {
                DebugSupport.SourceMap.Add(this, new SourceRange() { Path = path, StartPoint = new SourcePoint() { LineIndex = line, CharIndex = 0 }, EndPoint = new SourcePoint() { LineIndex = line + 1, CharIndex = 0 }, });
            }
        }
    }
}
