// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Debugging;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    /// <summary>
    /// Allow inspecting/modifying the current dialog context.
    /// </summary>
    /// <param name="inspector">Inspector for looking at current dialog context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public delegate Task Inspector(DialogContextInspector inspector);

    /// <summary>
    /// Abstract base class for scripted actions.
    /// </summary>
    public abstract class TestAction
    {
        /// <summary>
        /// Execute the test.
        /// </summary>
        /// <param name="adapter">Adapter to execute against.</param>
        /// <param name="callback">Logic for the bot to use.</param>
        /// <param name="inspector">Inspector for dialog context.</param>
        /// <returns>async task.</returns>
        public abstract Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback, Inspector inspector = null);

        /// <summary>
        /// Registers the path to file and callers line.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="line">The callers line.</param>
        protected void RegisterSourcePath(string path, int line)
        {
            if (!string.IsNullOrEmpty(path))
            {
                DebugSupport.SourceMap.Add(this, new SourceRange() { Path = path, StartPoint = new SourcePoint() { LineIndex = line, CharIndex = 0 }, EndPoint = new SourcePoint() { LineIndex = line + 1, CharIndex = 0 }, });
            }
        }
    }
}
