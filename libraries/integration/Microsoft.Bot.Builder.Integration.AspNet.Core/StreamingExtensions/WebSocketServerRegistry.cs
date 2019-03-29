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
                return false;
            }
        }

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
