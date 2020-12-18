// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging.CodeModels;
using Microsoft.Bot.Builder.Dialogs.Debugging.DataModels;
using Microsoft.Bot.Builder.Dialogs.Debugging.Events;
using Microsoft.Bot.Builder.Dialogs.Debugging.Identifiers;
using Microsoft.Bot.Builder.Dialogs.Debugging.Protocol;
using Microsoft.Bot.Builder.Dialogs.Debugging.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// Class which implements Debug Adapter protocol connected to IDialogDebugger data.
    /// </summary>
    internal sealed class DialogDebugAdapter : IMiddleware, IDialogDebugger, IDebugger
    {
        // https://en.wikipedia.org/wiki/Region-based_memory_management
        private readonly IIdentifier<ArenaModel> _arenas = new Identifier<ArenaModel>().WithMutex();

        private readonly IDebugTransport _transport;
        private readonly IBreakpoints _breakpoints;
        private readonly ICodeModel _codeModel;
        private readonly IDataModel _dataModel;
        private readonly IEvents _events;
        private readonly OutputModel _output = new OutputModel();
        private readonly ISourceMap _sourceMap;

        private readonly Action _terminate;
        private readonly ILogger _logger;

        // lifetime scoped to IMiddleware.OnTurnAsync
        private readonly ConcurrentDictionary<string, ThreadModel> _threadByTurnId = new ConcurrentDictionary<string, ThreadModel>();
        private readonly IIdentifier<ThreadModel> _threads = new Identifier<ThreadModel>().WithMutex();

        private LaunchAttach _options = new LaunchAttach();
        private int _sequence;

        public DialogDebugAdapter(IDebugTransport transport, ISourceMap sourceMap, IBreakpoints breakpoints, Action terminate, IEvents events = null, ICodeModel codeModel = null, IDataModel dataModel = null, ILogger logger = null, ICoercion coercion = null)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _events = events ?? new Events<DialogEvents>();
            _codeModel = codeModel ?? new CodeModel();
            _dataModel = dataModel ?? new DataModel(coercion ?? new Coercion());
            _sourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));
            _breakpoints = breakpoints ?? throw new ArgumentNullException(nameof(breakpoints));
            _terminate = terminate ?? (() => Environment.Exit(0));
            _logger = logger ?? NullLogger.Instance;
            _arenas.Add(_output);

            // lazily complete circular dependency
            _transport.Accept = AcceptAsync;
        }

        /// <summary>
        /// Thread debugging phases.
        /// </summary>
        public enum Phase
        {
            /// <summary>
            /// "Started" signals Visual Studio Code that there is a new thread.
            /// </summary>
            Started,

            /// <summary>
            /// Follows "Next".
            /// </summary>
            Continue,

            /// <summary>
            /// Signal to "Step" or to "Continue".
            /// </summary>
            Next,

            /// <summary>
            /// Follows "Next".
            /// </summary>
            Step,

            /// <summary>
            /// At breakpoint?
            /// </summary>
            Breakpoint,

            /// <summary>
            /// Thread paused.
            /// </summary>
            Pause,

            /// <summary>
            /// Thread exited.
            /// </summary>
            Exited
        }

        private int NextSeq => Interlocked.Increment(ref _sequence);

        public async Task OutputAsync(string text, object item, object value, CancellationToken cancellationToken)
        {
            var found = _sourceMap.TryGetValue(item, out var range);
            var code = EncodeValue(_output, value);

            var body = new
            {
                output = text + Environment.NewLine,
                source = found ? new Source(range.Path) : null,
                line = found ? (int?)range.StartPoint.LineIndex : null,
                variablesReference = code,
            };

            await SendAsync(Event.From(NextSeq, "output", body), cancellationToken).ConfigureAwait(false);
        }

        async Task IDialogDebugger.StepAsync(DialogContext context, object item, string more, CancellationToken cancellationToken)
        {
            try
            {
                var activity = context.Context.Activity;
                var turnText = activity.Text?.Trim() ?? string.Empty;
                if (turnText.Length == 0)
                {
                    turnText = activity.Type;
                }

                var threadText = $"'{StringUtils.Ellipsis(turnText, 18)}'";
                await OutputAsync($"{threadText} ==> {more?.PadRight(16) ?? string.Empty} ==> {_codeModel.NameFor(item)} ", item, null, cancellationToken).ConfigureAwait(false);

                await UpdateBreakpointsAsync(cancellationToken).ConfigureAwait(false);

                if (_threadByTurnId.TryGetValue(TurnIdFor(context.Context), out var thread))
                {
                    thread.SetLast(context, item, more);

                    var run = thread.Run;
                    if (_breakpoints.IsBreakPoint(item) && _events[more])
                    {
                        run.Post(Phase.Breakpoint);
                    }

                    if (_options.BreakOnStart && thread.StepCount == 0 && _events[more])
                    {
                        run.Post(Phase.Breakpoint);
                    }

                    ++thread.StepCount;

                    // TODO: implement asynchronous condition variables
                    Monitor.Enter(run.Gate);
                    try
                    {
                        // TODO: remove synchronous waits
                        UpdateThreadPhaseAsync(thread, item, cancellationToken).GetAwaiter().GetResult();

                        // while the stopped condition is true, atomically release the mutex
                        while (!(run.Phase == Phase.Started || run.Phase == Phase.Continue || run.Phase == Phase.Next))
                        {
                            Monitor.Wait(run.Gate);
                        }

                        // "Started" signals to Visual Studio Code that there is a new thread
                        if (run.Phase == Phase.Started)
                        {
                            run.Phase = Phase.Continue;
                        }

                        // TODO: remove synchronous waits
                        UpdateThreadPhaseAsync(thread, item, cancellationToken).GetAwaiter().GetResult();

                        // allow one step to progress since next was requested
                        if (run.Phase == Phase.Next)
                        {
                            run.Phase = Phase.Step;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(run.Gate);
                    }
                }
                else
                {
                    _logger.LogError("thread context not found");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types (we just log the exception and we continue the execution)
            catch (Exception error)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(error, error.Message);
            }
        }

        async Task IMiddleware.OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            var thread = new ThreadModel(turnContext, _codeModel);
            _arenas.Add(thread);
            _threads.Add(thread);
            _threadByTurnId.TryAdd(TurnIdFor(turnContext), thread);
            try
            {
                thread.Run.Post(Phase.Started);
                await UpdateThreadPhaseAsync(thread, null, cancellationToken).ConfigureAwait(false);

                IDialogDebugger trace = this;
                turnContext.TurnState.Add(trace);
                await next(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                thread.Run.Post(Phase.Exited);
                await UpdateThreadPhaseAsync(thread, null, cancellationToken).ConfigureAwait(false);

                _threadByTurnId.TryRemove(TurnIdFor(turnContext), out var ignored);
                _threads.Remove(thread);
                _arenas.Remove(thread);
            }
        }

        private static string TurnIdFor(ITurnContext turnContext)
        {
            return $"{turnContext.Activity.ChannelId}-{turnContext.Activity.Id}";
        }

        private async Task AcceptAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var token = await _transport.ReadAsync(cancellationToken).ConfigureAwait(false);
                    var request = ProtocolMessage.Parse(token);
                    Message message;
                    try
                    {
                        message = await DispatchAsync(request, cancellationToken).ConfigureAwait(false);
                    }
#pragma warning disable CA1031 // Do not catch general exception types (catch any exception and return it in the message)
                    catch (Exception error)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        message = Response.Fail(NextSeq, request, error.Message);
                    }

                    await SendAsync(message, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception error)
                {
                    _logger.LogError(error, error.Message);

                    ResetOnDisconnect();

                    throw;
                }
            }
        }

        private ulong EncodeValue(ArenaModel arena, object value)
        {
            if (_dataModel.IsScalar(value))
            {
                return 0;
            }

            var arenaCode = _arenas[arena];
            var valueCode = arena.ValueCodes.Add(value);
            return Identifier.Encode(arenaCode, valueCode);
        }

        private void DecodeValue(ulong variablesReference, out ArenaModel arena, out object value)
        {
            Identifier.Decode(variablesReference, out var threadCode, out var valueCode);
            arena = _arenas[threadCode];
            value = arena.ValueCodes[valueCode];
        }

        private ulong EncodeFrame(ThreadModel thread, ICodePoint frame)
        {
            var threadCode = _threads[thread];
            var valueCode = thread.FrameCodes.Add(frame);
            return Identifier.Encode(threadCode, valueCode);
        }

        private void DecodeFrame(ulong frameCode, out ThreadModel thread, out ICodePoint frame)
        {
            Identifier.Decode(frameCode, out var threadCode, out var valueCode);
            thread = _threads[threadCode];
            frame = thread.FrameCodes[valueCode];
        }

        private void ResetOnDisconnect()
        {
            // consider resetting this.events filter enabled state to defaults from constructor

            _options = new LaunchAttach();
            _breakpoints.Clear();
            _output.ValueCodes.Clear();
            ContinueAllThreads();
        }

        private void ContinueAllThreads()
        {
            var errors = new List<Exception>();
            foreach (var thread in _threads)
            {
                try
                {
                    thread.Value.Run.Post(Phase.Continue);
                }
#pragma warning disable CA1031 // Do not catch general exception types (catch any exception and add it to the aggregated list)
                catch (Exception error)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    errors.Add(error);
                }
            }

            if (errors.Count > 0)
            {
                throw new AggregateException(errors);
            }
        }

        private async Task UpdateBreakpointsAsync(CancellationToken cancellationToken)
        {
            var breakpoints = _breakpoints.ApplyUpdates();
            foreach (var breakpoint in breakpoints)
            {
                if (breakpoint.Verified)
                {
                    var item = _breakpoints.ItemFor(breakpoint);
                    await OutputAsync($"Set breakpoint at {_codeModel.NameFor(item)}", item, null, cancellationToken).ConfigureAwait(false);
                }

                var body = new
                {
                    reason = "changed",
                    breakpoint
                };
                await SendAsync(Event.From(NextSeq, "breakpoint", body), cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateThreadPhaseAsync(ThreadModel thread, object item, CancellationToken cancellationToken)
        {
            var run = thread.Run;
            if (run.Phase == run.PhaseSent)
            {
                return;
            }

            var phase = run.Phase;
            var suffix = item != null ? $" ==> {_codeModel.NameFor(item)}" : string.Empty;
            var threadText = $"{StringUtils.Ellipsis(thread?.Name, 18)}";
            if (threadText.Length <= 2)
            {
                threadText = thread.TurnContext.Activity.Type;
            }

            var description = $"{threadText} ==> {phase.ToString().PadRight(16)}{suffix}";

            await OutputAsync(description, item, null, cancellationToken).ConfigureAwait(false);

            var threadId = _threads[thread];

            if (phase == Phase.Next)
            {
                phase = Phase.Continue;
            }

            var reason = phase.ToString().ToLowerInvariant();

            if (phase == Phase.Started || phase == Phase.Exited)
            {
                await SendAsync(Event.From(NextSeq, "thread", new { threadId, reason }), cancellationToken).ConfigureAwait(false);
            }
            else if (phase == Phase.Continue)
            {
                await SendAsync(Event.From(NextSeq, "continued", new { threadId, allThreadsContinued = false }), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var body = new
                {
                    reason,
                    description,
                    threadId,
                    text = description,
                    preserveFocusHint = false,
                    allThreadsStopped = false,
                };

                await SendAsync(Event.From(NextSeq, "stopped", body), cancellationToken).ConfigureAwait(false);
            }

            run.PhaseSent = run.Phase;
        }

        private async Task SendAsync(Message message, CancellationToken cancellationToken)
        {
            var token = ProtocolMessage.ToToken(message);
            await _transport.SendAsync(token, cancellationToken).ConfigureAwait(false);
        }

        private Capabilities MakeCapabilities()
        {
            // TODO: there is a "capabilities" event for dynamic updates, but exceptionBreakpointFilters does not seem to be dynamically updateable
            return new Capabilities
            {
                SupportsConfigurationDoneRequest = true,
                SupportsSetVariable = true,
                SupportsEvaluateForHovers = true,
                SupportsFunctionBreakpoints = true,
                ExceptionBreakpointFilters = _events.Filters,
                SupportTerminateDebuggee = _terminate != null,
                SupportsTerminateRequest = _terminate != null,
            };
        }

        private async Task<Message> DispatchAsync(Message message, CancellationToken cancellationToken)
        {
            if (message is Request request)
            {
                if (message is Request<Initialize> initialize)
                {
                    var body = MakeCapabilities();
                    var response = Response.From(NextSeq, initialize, body);
                    await SendAsync(response, cancellationToken).ConfigureAwait(false);
                    return Event.From(NextSeq, "initialized", new { });
                }

                if (message is Request<Launch> launch)
                {
                    _options = launch.Arguments;
                    return Response.From(NextSeq, launch, new { });
                }

                if (message is Request<Attach> attach)
                {
                    _options = attach.Arguments;
                    return Response.From(NextSeq, attach, new { });
                }

                if (message is Request<SetBreakpoints> setBreakpoints)
                {
                    var arguments = setBreakpoints.Arguments;
                    var file = Path.GetFileName(arguments.Source.Path);
                    await OutputAsync($"Set breakpoints for {file}", null, null, cancellationToken).ConfigureAwait(false);

                    var breakpoints = _breakpoints.SetBreakpoints(arguments.Source, arguments.Breakpoints);
                    foreach (var breakpoint in breakpoints)
                    {
                        if (breakpoint.Verified)
                        {
                            var item = _breakpoints.ItemFor(breakpoint);
                            await OutputAsync($"Set breakpoint at {_codeModel.NameFor(item)}", item, null, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    return Response.From(NextSeq, setBreakpoints, new { breakpoints });
                }

                if (message is Request<SetFunctionBreakpoints> setFunctionBreakpoints)
                {
                    var arguments = setFunctionBreakpoints.Arguments;
                    await OutputAsync("Set function breakpoints.", null, null, cancellationToken).ConfigureAwait(false);
                    var breakpoints = _breakpoints.SetBreakpoints(arguments.Breakpoints);
                    foreach (var breakpoint in breakpoints)
                    {
                        if (breakpoint.Verified)
                        {
                            var item = _breakpoints.ItemFor(breakpoint);
                            await OutputAsync($"Set breakpoint at {_codeModel.NameFor(item)}", item, null, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    return Response.From(NextSeq, setFunctionBreakpoints, new { breakpoints });
                }

                if (message is Request<SetExceptionBreakpoints> setExceptionBreakpoints)
                {
                    var arguments = setExceptionBreakpoints.Arguments;
                    _events.Reset(arguments.Filters);

                    return Response.From(NextSeq, setExceptionBreakpoints, new { });
                }

                if (message is Request<Threads> threads)
                {
                    var body = new
                    {
                        threads = _threads.Select(t => new
                        {
                            id = t.Key,
                            name = t.Value.Name
                        }).ToArray()
                    };

                    return Response.From(NextSeq, threads, body);
                }

                if (message is Request<StackTrace> stackTrace)
                {
                    var arguments = stackTrace.Arguments;
                    var thread = _threads[arguments.ThreadId];

                    var frames = thread.Frames;
                    var stackFrames = new List<StackFrame>();
                    foreach (var frame in frames)
                    {
                        var stackFrame = new StackFrame
                        {
                            Id = EncodeFrame(thread, frame),
                            Name = frame.Name
                        };

                        var item = _codeModel.NameFor(frame.Item);
                        DebuggerSourceMap.Assign(stackFrame, item, frame.More);

                        if (_sourceMap.TryGetValue(frame.Item, out var range))
                        {
                            DebuggerSourceMap.Assign(stackFrame, range);
                        }

                        stackFrames.Add(stackFrame);
                    }

                    return Response.From(NextSeq, stackTrace, new { stackFrames });
                }

                if (message is Request<Scopes> scopes)
                {
                    var arguments = scopes.Arguments;
                    DecodeFrame(arguments.FrameId, out var thread, out var frame);
                    const bool expensive = false;

                    var body = new
                    {
                        scopes = new[]
                        {
                            new
                            {
                                expensive,
                                name = frame.Name,
                                variablesReference = EncodeValue(thread, frame.Data)
                            }
                        }
                    };

                    return Response.From(NextSeq, scopes, body);
                }

                if (message is Request<Variables> vars)
                {
                    var arguments = vars.Arguments;
                    DecodeValue(arguments.VariablesReference, out var arena, out var context);

                    var names = _dataModel.Names(context);

                    var body = new
                    {
                        variables = (from name in names
                                     let value = _dataModel[context, name]
                                     let variablesReference = EncodeValue(arena, value)
                                     select new
                                     {
                                         name = _dataModel.ToString(name),
                                         value = _dataModel.ToString(value),
                                         variablesReference
                                     })
                            .ToArray()
                    };

                    return Response.From(NextSeq, vars, body);
                }

                if (message is Request<SetVariable> setVariable)
                {
                    var arguments = setVariable.Arguments;
                    DecodeValue(arguments.VariablesReference, out var arena, out var context);

                    var value = _dataModel[context, arguments.Name] = JToken.Parse(arguments.Value);

                    var body = new
                    {
                        value = _dataModel.ToString(value),
                        variablesReference = EncodeValue(arena, value)
                    };

                    return Response.From(NextSeq, setVariable, body);
                }

                if (message is Request<Evaluate> evaluate)
                {
                    var arguments = evaluate.Arguments;
                    DecodeFrame(arguments.FrameId, out var thread, out var frame);
                    var expression = arguments.Expression.Trim('"');

                    try
                    {
                        var result = frame.Evaluate(expression);
                        var body = new
                        {
                            result = _dataModel.ToString(result),
                            variablesReference = EncodeValue(thread, result),
                        };

                        return Response.From(NextSeq, evaluate, body);
                    }
#pragma warning disable CA1031 // Do not catch general exception types (catch any exception and return it)
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        return Response.Fail(NextSeq, evaluate, ex.Message);
                    }
                }

                Response Post<TBody>(PerThread perThread, Phase phase, TBody body)
                {
                    // "constructing the response" and "posting to the thread" have side-effects
                    // for extra determinism, construct the response before signaling the thread
                    var response = Response.From(NextSeq, request, body);

                    var found = _threads.TryGetValue(perThread.ThreadId, out var thread);
                    if (found)
                    {
                        thread.Run.Post(phase);
                    }

                    return response;
                }

                if (message is Request<Continue> cont)
                {
                    return Post(cont.Arguments, Phase.Continue, new { allThreadsContinued = false });
                }

                if (message is Request<Pause> pause)
                {
                    return Post(pause.Arguments, Phase.Pause, new { });
                }

                if (message is Request<Next> next)
                {
                    return Post(next.Arguments, Phase.Next, new { });
                }

                if (message is Request<Terminate> terminate)
                {
                    if (_terminate != null)
                    {
                        _terminate();
                    }

                    return Response.From(NextSeq, terminate, new { });
                }

                if (message is Request<Disconnect> disconnect)
                {
                    var arguments = disconnect.Arguments;
                    if (arguments.TerminateDebuggee && _terminate != null)
                    {
                        _terminate();
                    }
                    else
                    {
                        ResetOnDisconnect();
                    }

                    return Response.From(NextSeq, disconnect, new { });
                }

                return Response.From(NextSeq, request, new { });
            }

            if (message is Event @event)
            {
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        private class ArenaModel
        {
            protected ArenaModel(IIdentifier<object> valueCodes)
            {
                ValueCodes = valueCodes ?? throw new ArgumentNullException(nameof(valueCodes));
            }

            public IIdentifier<object> ValueCodes { get; }
        }

        private sealed class OutputModel : ArenaModel
        {
            public OutputModel()
                : base(new Identifier<object>().WithCache(count: 25).WithMutex())
            {
            }
        }

        private sealed class ThreadModel : ArenaModel
        {
            public ThreadModel(ITurnContext turnContext, ICodeModel codeModel)
                : base(new Identifier<object>().WithMutex())
            {
                TurnContext = turnContext;
                CodeModel = codeModel;
            }

            public ITurnContext TurnContext { get; }

            public ICodeModel CodeModel { get; }

            public int StepCount { get; set; }

            public string Name => TurnContext.Activity.Text;

            public IReadOnlyList<ICodePoint> Frames
            {
                get
                {
                    // try to avoid regenerating Identifier values within a breakpoint
                    if (LastFrames == null)
                    {
                        LastFrames = CodeModel.PointsFor(LastContext, LastItem, LastMore);
                    }

                    return LastFrames;
                }
            }

            public RunModel Run { get; } = new RunModel();

            public IIdentifier<ICodePoint> FrameCodes { get; } = new Identifier<ICodePoint>().WithMutex();

            public DialogContext LastContext { get; private set; }

            public object LastItem { get; private set; }

            public string LastMore { get; private set; }

            private IReadOnlyList<ICodePoint> LastFrames { get; set; }

            public void SetLast(DialogContext context, object item, string more)
            {
                LastContext = context;
                LastItem = item;
                LastMore = more;

                LastFrames = null;
            }
        }
    }
}
