// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Debugging;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Mocks
{
    /// <summary>
    /// Helper class for mocks that want to be loaded from file and don't have variations.
    /// </summary>
    /// <typeparam name="T">The actual mock implementation.</typeparam>
    public abstract class MockData<T>
        where T : MockData<T>
    {
        public T Data => (T)this;

        protected void RegisterSourcePath(string path, int line)
        {
            if (!string.IsNullOrEmpty(path))
            {
                DebugSupport.SourceMap.Add(this, new SourceRange() { Path = path, StartPoint = new SourcePoint() { LineIndex = line, CharIndex = 0 }, EndPoint = new SourcePoint() { LineIndex = line + 1, CharIndex = 0 }, });
            }
        }
    }
}
