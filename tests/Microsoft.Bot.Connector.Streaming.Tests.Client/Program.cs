// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Streaming.Tests.Client
{
    public class Program
    {
        private static StreamingTransportClient _client;
        private static Task _clientTask;
        private static CancellationTokenSource _cancellationSource;
        private static string _conversationId;

        public static void Main(string[] args)
        {
            Menu();

            do
            {
                try
                {
                    DispatchAsync(Console.ReadLine()).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var originalForegroundColor = Console.ForegroundColor;
                    WriteLine($"Error: {ex}", ConsoleColor.Red);
                }
            }
            while (true);
        }

        private static async Task DispatchAsync(string command)
        {
            switch (command)
            {
                case "c":
                    await ConnectAsync();
                    break;

                case "cp":
                    await ConnectAsync(useNamedPipes: true);
                    break;

                case "car":
                    await ConnectAsync(automaticallyReconnect: true);
                    break;

                case "m":
                    await MessageAsync();
                    break;

                case "msplit":
                    await MessageSplitAsync();
                    break;

                case "sd":
                    await ForceServerDisconnectAsync();
                    break;

                case "d":
                    await DisconnectClientAsync();
                    break;

                case "h":
                    Menu();
                    break;
            }
        }

        private static string AskUser(string message)
        {
            Console.WriteLine(message);
            return Console.ReadLine();
        }

        private static async Task ConnectAsync(bool automaticallyReconnect = false, bool useNamedPipes = false)
        {
            var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>(string.Empty, null);
            var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new[] { configureNamedOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
            var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());

            // Improvement opportunity: expose command / argument to control log level.
            var loggerFactory = new LoggerFactory(new[] { new ConsoleLoggerProvider(optionsMonitor) }, new LoggerFilterOptions { MinLevel = LogLevel.Debug });

            if (useNamedPipes)
            {
                var pipeName = AskUser("Pipe Name:");
                _client = new NamedPipeClient(pipeName, url: ".", new ConsoleRequestHandler(), logger: loggerFactory.CreateLogger("NamedPipeClient"));
                _client.Disconnected += Client_Disconnected;
                _clientTask = _client.ConnectAsync();
            }
            else
            {
                var url = AskUser("Bot url:");
                var appId = AskUser("Bot app id:");
                var appPassword = AskUser("Bot app password:");

                var headers = new Dictionary<string, string>() { { "channelId", "Test" } };

                if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(appPassword))
                {
                    var credentials = new MsalAppCredentials(appId, appPassword, null, appId);
                    var token = await credentials.GetTokenAsync();

                    headers.Add("Authorization", $"Bearer {token}");
                }

                _cancellationSource = new CancellationTokenSource();

                _client = new WebSocketClient(new ClientWebSocket(), url, new ConsoleRequestHandler(), logger: loggerFactory.CreateLogger("WebSocketClient"));
                _client.Disconnected += Client_Disconnected;
                _clientTask = _client.ConnectAsync(headers, _cancellationSource.Token);
            }
        }

        private static void Client_Disconnected(object sender, Bot.Streaming.Transport.DisconnectedEventArgs e)
        {
            WriteLine($"[Program] Client disconnected. Reason: {e?.Reason}.", foregroundColor: ConsoleColor.Yellow);
            var response = AskUser("Attempt to reconnect the existing connection? y / n");

            // Let the client gracefully finish
            WriteLine("[Program] Waiting for graceful completion...", foregroundColor: ConsoleColor.Yellow);
            _clientTask.GetAwaiter().GetResult();

            if (response == "y")
            {
                WriteLine("[Program] Reconnecting...");
                ConnectAsync().GetAwaiter().GetResult();
            }
            else
            {
                WriteLine("[Program] Client shut down completed gracefully");
            }
        }

        private static async Task MessageAsync()
        {
            if (_client == null || !_client.IsConnected)
            {
                WriteLine("[Program] Client is not connected, connect before sending messages.");
            }

            var text = AskUser("[Program] Enter text:");

            WriteLine($"[User]: {text}", ConsoleColor.Cyan);

            if (string.IsNullOrEmpty(_conversationId))
            {
                _conversationId = Guid.NewGuid().ToString();
            }

            var activity = new Schema.Activity()
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = "testUser" },
                Conversation = new ConversationAccount { Id = _conversationId },
                Recipient = new ChannelAccount { Id = "testBot" },
                ServiceUrl = "wss://InvalidServiceUrl/api/messages",
                ChannelId = "Test",
                Text = text,
            };

            var request = StreamingRequest.CreatePost("/api/messages", new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, "application/json"));

            var stopwatch = Stopwatch.StartNew();

            var response = await _client.SendAsync(request, CancellationToken.None);
        }

        private static Task MessageSplitAsync()
        {
            throw new NotImplementedException();
        }

        private static Task ForceServerDisconnectAsync()
        {
            throw new NotImplementedException();
        }

        private static async Task DisconnectClientAsync()
        {
            await _client.DisconnectAsync();
            if (_cancellationSource != null)
            {
                _cancellationSource.Cancel();

                _cancellationSource.Dispose();
                _cancellationSource = null;
            }

            await _clientTask;
        }

        private static void Menu()
        {
            Console.WriteLine("Welcome to the streaming client.");
            Console.WriteLine("Commands:");
            Console.WriteLine("c - Connect web socket client");
            Console.WriteLine("cp - Connect named pipe client");
            Console.WriteLine("car - Connect web socket client with automatic reconnect");
            Console.WriteLine("m - Send message activity to bot");
            Console.WriteLine("msplit - Send message activity to bot, split between request and stream, allowing commands in between.");
            Console.WriteLine("sd - Force server disconnect");
            Console.WriteLine("d - Disconnect client");
            Console.WriteLine("h - Help");
        }

        private static void WriteLine(string message, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            // Save original color.
            //var originalForegroundColor = Console.ForegroundColor;
            //var originalBackgroundColor = Console.BackgroundColor;

            // Set requested color.
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            // Write message.
            Console.WriteLine(message);

            // Restore original colors.
            Console.ResetColor();

            //var Console.ForegroundColor = originalForegroundColor;
            //var Console.BackgroundColor = originalBackgroundColor;
        }

        private class ConsoleRequestHandler : RequestHandler
        {
            public override async Task<StreamingResponse> ProcessRequestAsync(ReceiveRequest request, ILogger<RequestHandler> logger, object context = null, CancellationToken cancellationToken = default)
            {
                var response = await request.ReadBodyAsJsonAsync<Schema.Activity>().ConfigureAwait(false);
                System.Console.WriteLine($"[Bot]: {response?.Text}");
                return await Task.FromResult(StreamingResponse.OK()).ConfigureAwait(false);
            }
        }
    }
}
