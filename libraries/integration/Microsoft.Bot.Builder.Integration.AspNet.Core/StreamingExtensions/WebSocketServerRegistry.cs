using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Protocol.WebSockets;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.StreamingExtensions
{
    public static class WebSocketServerRegistry
    {
        private static ConcurrentDictionary<string, WebSocketServer> servers = new ConcurrentDictionary<string, WebSocketServer>();

        /// <summary>
        /// Attempts to add a new ID and WebSocketServer to the registry.
        /// </summary>
        /// <param name="serverId">The ID of the server to add.</param>
        /// <param name="server">The WebSocketServer to register.</param>
        /// <returns>True if successful, throws on failure.</returns>
        public static bool RegisterNewServer(string serverId, WebSocketServer server)
        {
            if (servers == null)
            {
                servers = new ConcurrentDictionary<string, WebSocketServer>();
            }

            try
            {
                servers.TryAdd(serverId, server);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Attempts to find a serverID in the registry.
        /// </summary>
        /// <param name="serverID">The ID of the server to find.</param>
        /// <returns>The WebSocketServer with the associated ID.</returns>
        public static WebSocketServer GetServerByID(string serverID)
        {
            try
            {
                servers.TryGetValue(serverID, out var server);
                return server;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
