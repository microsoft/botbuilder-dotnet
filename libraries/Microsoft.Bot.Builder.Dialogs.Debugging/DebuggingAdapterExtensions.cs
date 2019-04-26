using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public static class DebbugingBotAdapterExtensions
    {
        /// <summary>
        /// Enable Debug Adapter Protocol for the running adapter.
        /// </summary>
        /// <param name="botAdapter">BotAdapter to enable</param>
        /// <param name="port">port to listen on</param>
        /// <param name="registry">IRegistry to use (default will be SourceMap())</param>
        /// <param name="breakpoints">IBreakpoints to use (default will be SourceMap())</param>
        /// <param name="terminate">Termination function (Default is Environment.Exit()</param>
        /// <param name="logger">ILogger to use (Default is NullLogger)</param>
        /// <param name="codeModel">ICodeModel to use (default is internal implementation)</param>
        /// <param name="dataModel">IDataModel to use (default is internal implementation)</param>
        /// <param name="coercion">ICoercion to use (default is internal implementation)</param>
        /// <returns></returns>
        public static BotAdapter UseDebugger(this BotAdapter botAdapter, int port, Source.IRegistry registry = null, IBreakpoints breakpoints = null, Action terminate = null, ICodeModel codeModel = null, IDataModel dataModel = null, ILogger logger = null, ICoercion coercion = null)
        {
            codeModel = codeModel ?? new CodeModel();
            var sourceMap = new SourceMap(codeModel);
            DebugSupport.SourceRegistry = registry ?? sourceMap;
            return botAdapter.Use(new DebugAdapter(port: port, 
                registry: registry ?? sourceMap, 
                breakpoints: breakpoints ?? registry as IBreakpoints ?? sourceMap, 
                terminate: terminate, 
                codeModel: codeModel,
                dataModel: dataModel, 
                logger: logger));
        }


    }
}
