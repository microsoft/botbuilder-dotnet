// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Transport;
using Microsoft.Bot.StreamingExtensions.Transport.NamedPipes;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder
{
    internal class NamedPipeConnector
    {
        /*  The default named pipe all instances of DL ASE listen on is named bfv4.pipes
            Unfortunately this name is no longer very discriptive, but for the time being
            we're unable to change it without coordinated updates to DL ASE, which we
            currently are unable to perform.
        */
        private const string DefaultPipeName = "bfv4.pipes";
        private readonly ILogger _logger;
        private readonly string _pipeName;

        internal NamedPipeConnector(ILogger logger = null, string pipeName = DefaultPipeName)
        {
            _logger = logger;
            _pipeName = pipeName;
        }

        internal void InitializeNamedPipeServer(IBot bot, IList<IMiddleware> middleware = null, Func<ITurnContext, Exception, Task> onTurnError = null)
        {
            var handler = new StreamingRequestHandler(onTurnError, bot, middleware);

            try
            {
                IStreamingTransportServer server = new NamedPipeServer(_pipeName, handler);
                handler.Server = server;

                Task.Run(() => server.StartAsync());
            }
            catch (Exception ex)
            {
                /* The inability to establish a named pipe connection is not a terminal condition,
                 * and should not interrupt the bot's initialization sequence. We log the failure
                 * as informative but do not throw an exception or cause a disruption to the bot,
                 * as either would require developers to spend time and effort on a feature they
                 * may not care about or intend to make use of.
                 * As our support for named pipe bots evolves we will likely be able to restrict
                 * connection attempts to when they're likely to succeed, but for now it's possible
                 * a bot will check for a named pipe connection, find that one does not exist, and
                 * simply continue to serve as an HTTP and/or WebSocket bot, none the wiser.
                 */
                _logger?.LogInformation(string.Format("Failed to create server: {0}", ex));
            }
        }
    }
}
