// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public static class DebuggingBotAdapterExtensions
    {
        /// <summary>
        /// Enable Debug Adapter Protocol for the running adapter.
        /// </summary>
        /// <param name="botAdapter">The <see cref="BotAdapter"/> to enable.</param>
        /// <param name="port">port to listen on.</param>
        /// <param name="sourceMap">ISourceMap to use (default will be SourceMap()).</param>
        /// <param name="breakpoints">IBreakpoints to use (default will be SourceMap()).</param>
        /// <param name="terminate">Termination function (Default is Environment.Exit().</param>
        /// <param name="events">IEvents to use (Default is Events).</param>
        /// <param name="codeModel">ICodeModel to use (default is internal implementation).</param>
        /// <param name="dataModel">IDataModel to use (default is internal implementation).</param>
        /// <param name="logger">ILogger to use (Default is NullLogger).</param>
        /// <param name="coercion">ICoercion to use (default is internal implementation).</param>
        /// <returns>The <see cref="BotAdapter"/>.</returns>
        public static BotAdapter UseDebugger(
            this BotAdapter botAdapter, 
            int port, 
            ISourceMap sourceMap = null, 
            IBreakpoints breakpoints = null, 
            Action terminate = null, 
            IEvents events = null, 
            ICodeModel codeModel = null, 
            IDataModel dataModel = null, 
            ILogger logger = null, 
            ICoercion coercion = null)
        {
            codeModel = codeModel ?? new CodeModel();
            DebugSupport.SourceMap = sourceMap ?? new DebuggerSourceMap(codeModel);

            return botAdapter.Use(
                new DialogDebugAdapter(
                    port: port, 
                    sourceMap: DebugSupport.SourceMap, 
                    breakpoints: breakpoints ?? DebugSupport.SourceMap as IBreakpoints,
                    terminate: terminate, 
                    events: events,
                    codeModel: codeModel,
                    dataModel: dataModel, 
                    logger: logger));
        }
    }
}
