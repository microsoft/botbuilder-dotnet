using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Transport;
using Microsoft.Bot.StreamingExtensions.Transport.NamedPipes;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder
{
    public class NamedPipeConnector
    {
        private readonly ILogger _logger;
        private readonly string _pipeName;

        public NamedPipeConnector(ILogger logger = null, string pipeName = "bfv4.pipes")
        {
            _logger = logger;
            _pipeName = pipeName;
        }

        public void InitializeNamedPipeServer(IBot bot, IList<IMiddleware> middleware = null, Func<ITurnContext, Exception, Task> onTurnError = null)
        {
            var handler = new StreamingRequestHandler(onTurnError, bot, middleware);
            IStreamingTransportServer server = null;
            _logger?.LogInformation("Creating server for Named Pipe connection.");
            server = new NamedPipeServer(_pipeName, handler);

            if (server == null)
            {
                _logger?.LogInformation("Failed to create server.");

                return;
            }

            handler.Server = server;

            Task.Run(() => server.StartAsync());

        }
    }
}
