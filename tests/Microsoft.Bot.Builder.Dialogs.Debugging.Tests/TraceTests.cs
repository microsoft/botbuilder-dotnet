using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Debugging.Base;
using Microsoft.Bot.Builder.Dialogs.Debugging.CodeModels;
using Microsoft.Bot.Builder.Dialogs.Debugging.DataModels;
using Microsoft.Bot.Builder.Dialogs.Debugging.Events;
using Microsoft.Bot.Builder.Dialogs.Debugging.Protocol;
using Microsoft.Bot.Builder.Dialogs.Debugging.Transport;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests
{
    public sealed class TraceTests
    {
        private readonly ITestOutputHelper _output;

        public TraceTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public async Task ProtocolMessages_AreConsistent()
        {
            WaterfallStep MakeStep(string text, DialogTurnResult result = null)
                => async (stepContext, cancellationToken) =>
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(text), cancellationToken);
                    return result ?? Dialog.EndOfTurn;
                };

            var dialog = new WaterfallDialog(
                nameof(ProtocolMessages_AreConsistent),
                new[] { MakeStep("hello"), MakeStep("world") });

            var trace = new List<JToken>();
            var transport = new MockTransport(trace);
            var debugger = MakeDebugger(transport);

            using (new ActiveObject(((IDebugTransport)transport).Accept))
            {
                var storage = new MemoryStorage();
                var userState = new UserState(storage);
                var conversationState = new ConversationState(storage);
                var adapter = new TestAdapter()
                    .Use(debugger)
                    .UseStorage(storage)
                    .UseBotState(userState, conversationState);

                var dialogManager = new DialogManager(dialog);

                await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
                {
                    await dialogManager.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
                })
                .Send("one")
                .AssertReply("hello")
                .Send("two")
                .AssertReply("world")
                .StartTestAsync();
            }

            var pathJson = TraceOracle.MakePath(nameof(ProtocolMessages_AreConsistent));
            await TraceOracle.ValidateAsync(pathJson, trace, _output);
        }

        internal static DialogDebugAdapter MakeDebugger(IDebugTransport transport)
        {
            var codeModel = new CodeModel();
            var sourceMap = new DebuggerSourceMap(codeModel);
            var events = new Events<DialogEvents>();
            var coercion = new Coercion();
            var dataModel = new DataModel(coercion);
            var debugger = new DialogDebugAdapter(transport, sourceMap, sourceMap, () => { }, events, codeModel, dataModel, NullLogger.Instance, coercion);
            return debugger;
        }

        internal sealed class MockTransport : IDebugTransport
        {
            private readonly List<JToken> _trace;
            private readonly Queue<JToken> _queue = new Queue<JToken>();
            private readonly SemaphoreSlim _count = new SemaphoreSlim(0, int.MaxValue);
            private readonly object _gate = new object();
            private int _seq = 0;

            public MockTransport(List<JToken> trace)
            {
                _trace = trace;

                Request(new Initialize() { });
            }

            Func<CancellationToken, Task> IDebugTransport.Accept { get; set; }

            async Task<JToken> IDebugTransport.ReadAsync(CancellationToken cancellationToken)
            {
                await _count.WaitAsync(cancellationToken).ConfigureAwait(false);
                lock (_gate)
                {
                    return _queue.Dequeue();
                }
            }

            Task IDebugTransport.SendAsync(JToken token, CancellationToken cancellationToken)
            {
                try
                {
                    lock (_gate)
                    {
                        _trace.Add(token);
                    }

                    var incoming = token.ToObject<Incoming>();
                    if (incoming.Type == "event")
                    {
                        switch (incoming.Event)
                        {
                            case "initialized":
                                Request(new Attach() { BreakOnStart = true });
                                Request(new ConfigurationDone());
                                Request(new Threads());
                                break;
                            case "stopped":
                                Request(new Next() { ThreadId = incoming.Body.ThreadId });
                                break;
                        }
                    }

                    return Task.CompletedTask;
                }
                catch (OperationCanceledException error)
                {
                    return Task.FromCanceled(error.CancellationToken);
                }
                catch (Exception error)
                {
                    return Task.FromException(error);
                }
            }

            private void Request<TBody>(TBody arguments)
            {
                var request = new Request<TBody>()
                {
                    Type = nameof(Request).ToLowerInvariant(),
                    Command = arguments.GetType().Name.ToLowerInvariant(),
                    Arguments = arguments,
                };

                Enqueue(request);
            }

            private void Enqueue(Message message)
            {
                lock (_gate)
                {
                    message.Seq = ++_seq;
                    var token = ProtocolMessage.ToToken(message);
                    _trace.Add(token);
                    _queue.Enqueue(token);
                }

                _count.Release();
            }

            private sealed class Incoming
            {
                public string Type { get; set; }

                public string Command { get; set; }

                public string Event { get; set; }

                public BodyType Body { get; set; }

                public sealed class BodyType : HasRest
                {
                    public ulong ThreadId { get; set; }
                }
            }
        }
    }
}
