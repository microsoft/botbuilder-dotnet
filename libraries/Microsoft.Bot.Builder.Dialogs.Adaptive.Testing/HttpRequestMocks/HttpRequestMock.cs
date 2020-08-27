// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Debugging;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    /// <summary>
    /// Base class for all http request mocks.
    /// </summary>
    public abstract class HttpRequestMock
    {
        /// <summary>
        /// In the constructor of MockHttpRequestMiddleware, this function will be called to setup a global handler.
        /// </summary>
        /// <param name="handler">The global handler.</param>
        public abstract void Setup(MockHttpMessageHandler handler);

        /// <summary>
        /// helper to register source path with debugger.
        /// </summary>
        /// <param name="path">optional path.</param>
        /// <param name="line">optional line.</param>
        protected void RegisterSourcePath(string path, int line)
        {
            if (!string.IsNullOrEmpty(path))
            {
                DebugSupport.SourceMap.Add(this, new SourceRange() { Path = path, StartPoint = new SourcePoint() { LineIndex = line, CharIndex = 0 }, EndPoint = new SourcePoint() { LineIndex = line + 1, CharIndex = 0 }, });
            }
        }
    }
}
