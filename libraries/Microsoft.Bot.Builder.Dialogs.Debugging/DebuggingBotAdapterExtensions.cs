// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Debugging.CodeModels;
using Microsoft.Bot.Builder.Dialogs.Debugging.Transport;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// Defines debugging extension methods for the <see cref="BotAdapter"/> class.
    /// </summary>
    public static class DebuggingBotAdapterExtensions
    {
        /// <summary>
        /// Enable Debug Adapter Protocol for the running adapter.
        /// </summary>
        /// <param name="botAdapter">The <see cref="BotAdapter"/> to enable.</param>
        /// <param name="port">port to listen on.</param>
        /// <param name="sourceMap">ISourceMap to use (default will be SourceMap()).</param>
        /// <param name="terminate">Termination function (Default is Environment.Exit().</param>
        /// <param name="logger">ILogger to use (Default is NullLogger).</param>
        /// <returns>The <see cref="BotAdapter"/>.</returns>
        public static BotAdapter UseDebugger(
            this BotAdapter botAdapter,
            int port,
            ISourceMap sourceMap = null,
            Action terminate = null,
            ILogger logger = null)
        {
            DebugSupport.SourceMap = sourceMap ?? new DebuggerSourceMap(new CodeModel());

            return botAdapter.Use(
#pragma warning disable CA2000 // Dispose objects before losing scope (excluding, the object ownership is transferred to the adapter and the adapter should dispose it)
                new DialogDebugAdapter(
                    new DebugTransport(port, logger),
                    DebugSupport.SourceMap,
                    DebugSupport.SourceMap as IBreakpoints,
                    terminate,
                    codeModel: new CodeModel(),
                    logger: logger));
#pragma warning restore CA2000 // Dispose objects before losing scope
        }
    }
}
