// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Transport.NamedPipes;
using Microsoft.Bot.StreamingExtensions.UnitTests.Mocks;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Utilities
{
    public class NamedPipeFlow
    {
        private readonly NamedPipeServer _server;
        private readonly NamedPipeClient _client;
        private readonly int _clientTasks;
        private readonly int _serverTasks;
        private Dictionary<string, PendingRequest> _responses;

        private List<PendingAction> _sendToServer = new List<PendingAction>();
        private List<PendingAction> _sendToClient = new List<PendingAction>();

        public NamedPipeFlow(string pipeName = null, int clientTasks = 10, int serverTasks = 10)
        {
            pipeName = pipeName ?? Guid.NewGuid().ToString();
            _clientTasks = clientTasks;
            _serverTasks = serverTasks;
            _server = new NamedPipeServer(pipeName, new MockRequestHandler(async (r) => await ProcessRequest(r)), false);
            _client = new NamedPipeClient(pipeName, new MockRequestHandler(async (r) => await ProcessRequest(r)), false);
            _responses = new Dictionary<string, PendingRequest>();
            var t = Task.Run(_server.StartAsync);
            var t2 = Task.Run(_client.ConnectAsync);
        }

        public void SendToServer(StreamingRequest request, StreamingResponse expectedResponse, Func<StreamingRequest, StreamingResponse, ReceiveResponse, Task> validate, Func<ReceiveRequest, Task> validateRequest = null)
        {
            _responses.Add(GetRequestKey(request), new PendingRequest() { Response = expectedResponse, ValidateRequest = validateRequest });
            _sendToServer.Add(new PendingAction()
            {
                Request = request,
                ExpectedResponse = expectedResponse,
                Validate = validate,
                ToClient = false,
            });
        }

        public void SendToClient(StreamingRequest request, StreamingResponse expectedResponse, Func<StreamingRequest, StreamingResponse, ReceiveResponse, Task> validate, Func<ReceiveRequest, Task> validateRequest = null)
        {
            _responses.Add(GetRequestKey(request), new PendingRequest() { Response = expectedResponse, ValidateRequest = validateRequest });

            _sendToClient.Add(new PendingAction()
            {
                Request = request,
                ExpectedResponse = expectedResponse,
                Validate = validate,
                ToClient = true,
            });
        }

        public void Run()
        {
            try
            {
                while (_sendToClient.Count > 0 || _sendToServer.Count > 0)
                {
                    List<Task> tasks = new List<Task>();

                    var count = Math.Min(_clientTasks, _sendToServer.Count);

                    for (int i = 0; i < count; i++)
                    {
                        var a = _sendToServer[i];
                        tasks.Add(Task.Run(async () =>
                        {
                            ReceiveResponse receivedResponse;
                            receivedResponse = await _client.SendAsync(a.Request).ConfigureAwait(false);
                            await a.Validate(a.Request, a.ExpectedResponse, receivedResponse).ConfigureAwait(false);
                        }));
                    }

                    _sendToServer.RemoveRange(0, count);

                    count = Math.Min(_serverTasks, _sendToClient.Count);

                    for (int i = 0; i < count; i++)
                    {
                        var a = _sendToClient[i];
                        tasks.Add(Task.Run(async () =>
                        {
                            ReceiveResponse receivedResponse;
                            receivedResponse = await _server.SendAsync(a.Request).ConfigureAwait(false);
                            await a.Validate(a.Request, a.ExpectedResponse, receivedResponse).ConfigureAwait(false);
                        }));
                    }

                    _sendToClient.RemoveRange(0, count);

                    try
                    {
                        Task.WaitAll(tasks.ToArray(), -1);
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.InnerException;
                    }
                }
            }
            finally
            {
                _server.Disconnect();
                _client.Disconnect();
            }
        }

        private async Task<StreamingResponse> ProcessRequest(ReceiveRequest request)
        {
            StreamingResponse response = null;

            if (!_responses.TryGetValue(GetRequestKey(request), out PendingRequest pendingRequest))
            {
                response = new StreamingResponse()
                {
                    StatusCode = 500,
                };
            }
            else
            {
                response = pendingRequest.Response;
                if (pendingRequest.ValidateRequest != null)
                {
                    await pendingRequest.ValidateRequest(request);
                }
            }

            return response;
        }

        private string GetRequestKey(StreamingRequest request)
        {
            return $"{request.Verb}: {request.Path}";
        }

        private string GetRequestKey(ReceiveRequest request)
        {
            return $"{request.Verb}: {request.Path}";
        }

        private class PendingRequest
        {
            private StreamingResponse response;
            private Func<ReceiveRequest, Task> validateRequest;

            public StreamingResponse Response { get => response; set => response = value; }

            public Func<ReceiveRequest, Task> ValidateRequest { get => validateRequest; set => validateRequest = value; }
        }

        private class PendingAction
        {
            private StreamingRequest request;
            private StreamingResponse expectedResponse;
            private Func<StreamingRequest, StreamingResponse, ReceiveResponse, Task> validate;
            private bool toClient;
            private TaskCompletionSource<string> done = new TaskCompletionSource<string>();

            public bool ToClient { get => toClient; set => toClient = value; }

            public Func<StreamingRequest, StreamingResponse, ReceiveResponse, Task> Validate { get => validate; set => validate = value; }

            public TaskCompletionSource<string> Done { get => done; set => done = value; }

            public StreamingResponse ExpectedResponse { get => expectedResponse; set => expectedResponse = value; }

            public StreamingRequest Request { get => request; set => request = value; }
        }
    }
}
