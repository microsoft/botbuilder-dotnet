using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class DebugAdapter : DebugTransport, IMiddleware, DebugSupport.IDebugger
    {
        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private readonly Source.IRegistry registry;
        private readonly IBreakpoints breakpoints;

        // lifetime scoped to IMiddleware.OnTurnAsync
        private readonly ConcurrentDictionary<ITurnContext, TurnThreadModel> threadByContext = new ConcurrentDictionary<ITurnContext, TurnThreadModel>();
        private readonly Identifier<IThreadModel> threads = new Identifier<IThreadModel>();
        // TODO: leaks - consider scoping or ConditionalWeakTable
        private readonly Identifier<FrameModel> frames = new Identifier<FrameModel>();
        // TODO: leaks - consider scoping or ConditionalWeakTable
        private readonly Identifier<VariableModel> variables = new Identifier<VariableModel>();

        private interface IThreadModel
        {
            string Name { get; }
            IReadOnlyList<FrameModel> Frames { get; }
            RunModel Run { get; }
        }

        private sealed class BotThreadModel : IThreadModel
        {
            public string Name => "Bot";
            public IReadOnlyList<FrameModel> Frames => Array.Empty<FrameModel>();
            public RunModel Run { get; } = new RunModel();
        }

        private sealed class TurnThreadModel : IThreadModel
        {
            public TurnThreadModel(ITurnContext turnContext)
            {
                TurnContext = turnContext;
            }
            public string Name => TurnContext.Activity.Text;

            public IReadOnlyList<FrameModel> Frames => Model.FramesFor(LastContext, LastItem, LastMore);
            public RunModel Run { get; } = new RunModel();
            public ITurnContext TurnContext { get; }
            public DialogContext LastContext { get; set; }
            public object LastItem { get; set; }
            public string LastMore { get; set; }
        }

        public enum Phase { Started, Continue, Next, Step, Breakpoint, Pause, Exited };

        public sealed class RunModel
        {
            public Phase? PhaseSent { get; set; }
            public Phase Phase { get; set; } = Phase.Started;
            public object Gate { get; } = new object();

            public void Post(Phase what)
            {
                Monitor.Enter(Gate);
                try
                {
                    Phase = what;
                    Monitor.Pulse(Gate);
                }
                finally
                {
                    Monitor.Exit(Gate);
                }
            }
        }

        private int VariablesReference(VariableModel variable)
        {
            var value = variable.Value;
            if (Policy.ShowAsScalar(variable.Value))
            {
                return 0;
            }

            return variables.Add(variable);
        }

        private readonly Task task;

        public DebugAdapter(Source.IRegistry registry, IBreakpoints breakpoints, ILogger logger)
            : base(logger)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            this.breakpoints = breakpoints ?? throw new ArgumentNullException(nameof(breakpoints));
            this.task = ListenAsync(new IPEndPoint(IPAddress.Any, port: 4712), cancellationToken.Token);
            //threads.Add(new BotThreadModel());
        }

        public async Task DisposeAsync()
        {
            this.cancellationToken.Cancel();
            using (this.cancellationToken)
            using (this.task)
            {
                await this.task.ConfigureAwait(false);
            }
        }

        async Task IMiddleware.OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            //if (System.Diagnostics.Debugger.IsAttached)
            //{
            //    var source = new CancellationTokenSource();
            //    source.CancelAfter(TimeSpan.FromMinutes(2));
            //    cancellationToken = source.Token;
            //}

            var thread = new TurnThreadModel(turnContext);
            var threadId = threads.Add(thread);
            threadByContext.TryAdd(turnContext, thread);
            try
            {
                thread.Run.Post(Phase.Started);
                await UpdateThreadPhaseAsync(thread, null, cancellationToken).ConfigureAwait(false);

                DebugSupport.IDebugger trace = this;
                turnContext.TurnState.Add(trace);
                await next(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                thread.Run.Post(Phase.Exited);
                await UpdateThreadPhaseAsync(thread, null, cancellationToken).ConfigureAwait(false);

                threadByContext.TryRemove(turnContext, out var ignored);
                threads.Remove(thread);
            }
        }

        async Task DebugSupport.IDebugger.StepAsync(DialogContext context, object item, string more, CancellationToken cancellationToken)
        {
            try
            {
                if (item is Dialog)
                {
                    System.Diagnostics.Trace.TraceInformation($"{Policy.NameFor(item)} {((Dialog)item).Id} {more}");
                }
                await OutputAsync($"Step: {Policy.NameFor(item)} {more}", item, cancellationToken).ConfigureAwait(false);

                await UpdateBreakpointsAsync(cancellationToken).ConfigureAwait(false);

                var thread = threadByContext[context.Context];
                thread.LastContext = context;
                thread.LastItem = item;
                thread.LastMore = more;

                var run = thread.Run;
                if (breakpoints.IsBreakPoint(item))
                {
                    run.Post(Phase.Breakpoint);
                }

                // TODO: implement asynchronous condition variables
                Monitor.Enter(run.Gate);
                try
                {
                    // TODO: remove synchronous waits
                    UpdateThreadPhaseAsync(thread, item, cancellationToken).GetAwaiter().GetResult();

                    while (!(run.Phase == Phase.Started || run.Phase == Phase.Continue || run.Phase == Phase.Next))
                    {
                        Monitor.Wait(run.Gate);
                    }

                    if (run.Phase == Phase.Started)
                    {
                        run.Phase = Phase.Continue;
                    }

                    // TODO: remove synchronous waits
                    UpdateThreadPhaseAsync(thread, item, cancellationToken).GetAwaiter().GetResult();

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
            catch (Exception error)
            {
                this.logger.LogError(error, error.Message);
            }
        }

        private async Task UpdateBreakpointsAsync(CancellationToken cancellationToken)
        {
            var breakpoints = this.breakpoints.ApplyUpdates();
            foreach (var breakpoint in breakpoints)
            {
                if (breakpoint.verified)
                {
                    var item = this.breakpoints.ItemFor(breakpoint);
                    await OutputAsync($"Set breakpoint at {Policy.NameFor(item)}", item, cancellationToken).ConfigureAwait(false);
                }

                var body = new { reason = "changed", breakpoint };
                await SendAsync(Protocol.Event.From(NextSeq, "breakpoint", body), cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateThreadPhaseAsync(IThreadModel thread, object item, CancellationToken cancellationToken)
        {
            var run = thread.Run;
            if (run.Phase == run.PhaseSent)
            {
                return;
            }

            var phase = run.Phase;
            var suffix = item != null ? $" at {Policy.NameFor(item)}" : string.Empty;
            var description = $"'{thread.Name}' is {phase}{suffix}";

            await OutputAsync(description, item, cancellationToken).ConfigureAwait(false);

            var threadId = this.threads[thread];

            if (phase == Phase.Next)
            {
                phase = Phase.Continue;
            }

            string reason = phase.ToString().ToLower();

            if (phase == Phase.Started || phase == Phase.Exited)
            {
                await SendAsync(Protocol.Event.From(NextSeq, "thread", new { threadId, reason }), cancellationToken).ConfigureAwait(false);
            }
            else if (phase == Phase.Continue)
            {
                await SendAsync(Protocol.Event.From(NextSeq, "continue", new { threadId, allThreadsContinued = false }), cancellationToken).ConfigureAwait(false);
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
                    allThreadsStopped = true,
                };

                await SendAsync(Protocol.Event.From(NextSeq, "stopped", body), cancellationToken).ConfigureAwait(false);
            }

            run.PhaseSent = run.Phase;
        }

        private async Task SendAsync(Protocol.Message message, CancellationToken cancellationToken)
        {
            var token = JToken.FromObject(message, new JsonSerializer() { NullValueHandling = NullValueHandling.Include });
            await SendAsync(token, cancellationToken).ConfigureAwait(false);
        }

        private async Task OutputAsync(string text, object item, CancellationToken cancellationToken)
        {
            bool found = this.registry.TryGetValue(item, out var range);

            var body = new
            {
                output = text + Environment.NewLine,
                source = found ? new Protocol.Source(range.Path) : null,
                line = found ? (int?)range.Start.LineIndex : null,
            };

            await SendAsync(Protocol.Event.From(NextSeq, "output", body), cancellationToken).ConfigureAwait(false);
        }

        private int sequence = 0;
        private int NextSeq => Interlocked.Increment(ref sequence);

        private async Task<Protocol.Message> DispatchAsync(Protocol.Message message, CancellationToken cancellationToken)
        {
            if (message is Protocol.Request<Protocol.Initialize> initialize)
            {
                var body = new
                {
                    supportsConfigurationDoneRequest = true,
                };
                var response = Protocol.Response.From(NextSeq, initialize, body);
                await SendAsync(response, cancellationToken).ConfigureAwait(false);
                return Protocol.Event.From(NextSeq, "initialized", new { });
            }
            else if (message is Protocol.Request<Protocol.Launch> launch)
            {
                return Protocol.Response.From(NextSeq, launch, new { });
            }
            else if (message is Protocol.Request<Protocol.Attach> attach)
            {
                return Protocol.Response.From(NextSeq, attach, new { });
            }
            else if (message is Protocol.Request<Protocol.SetBreakpoints> setBreakpoints)
            {
                var arguments = setBreakpoints.arguments;
                var file = Path.GetFileName(arguments.source.path);
                await OutputAsync($"Set breakpoints for {file}", null, cancellationToken).ConfigureAwait(false);

                var breakpoints = this.breakpoints.SetBreakpoints(arguments.source, arguments.breakpoints);
                foreach (var breakpoint in breakpoints)
                {
                    if (breakpoint.verified)
                    {
                        var item = this.breakpoints.ItemFor(breakpoint);
                        await OutputAsync($"Set breakpoint at {Policy.NameFor(item)}", item, cancellationToken).ConfigureAwait(false);
                    }
                }

                return Protocol.Response.From(NextSeq, setBreakpoints, new { breakpoints });
            }
            else if (message is Protocol.Request<Protocol.Threads> threads)
            {
                var body = new
                {
                    threads = this.threads.Select(t => new { id = t.Key, name = t.Value.Name }).ToArray()
                };

                return Protocol.Response.From(NextSeq, threads, body);
            }
            else if (message is Protocol.Request<Protocol.StackTrace> stackTrace)
            {
                var arguments = stackTrace.arguments;
                var thread = this.threads[arguments.threadId];

                var frames = thread.Frames;
                var stackFrames = new List<Protocol.StackFrame>();
                foreach (var frame in frames)
                {
                    var stackFrame = new Protocol.StackFrame()
                    {
                        id = this.frames.Add(frame),
                        name = frame.Name
                    };

                    if (this.registry.TryGetValue(frame.Item, out var range))
                    {
                        stackFrame.source = new Protocol.Source(range.Path);
                        stackFrame.line = range.Start.LineIndex;
                        stackFrame.column = range.Start.CharIndex;
                        stackFrame.endLine = range.After.LineIndex;
                        stackFrame.endColumn = range.After.CharIndex;
                    }

                    stackFrames.Add(stackFrame);
                }

                return Protocol.Response.From(NextSeq, stackTrace, new { stackFrames });
            }
            else if (message is Protocol.Request<Protocol.Scopes> scopes)
            {
                var arguments = scopes.arguments;
                var frame = this.frames[arguments.frameId];
                const bool expensive = false;

                var body = new
                {
                    scopes = new[] { new { expensive, name = frame.Name, variablesReference = VariablesReference(frame.Scopes) } }
                };

                return Protocol.Response.From(NextSeq, scopes, body);
            }
            else if (message is Protocol.Request<Protocol.Variables> vars)
            {
                var arguments = vars.arguments;
                var variable = this.variables[arguments.variablesReference];
                var variables = Model.VariablesFor(variable);

                var body = new
                {
                    variables = variables.Select(v => new
                    {
                        name = v.Name,
                        value = Policy.ScalarJsonValue(v.Value),
                        variablesReference = VariablesReference(v)
                    }).ToArray()
                };

                return Protocol.Response.From(NextSeq, vars, body);
            }
            else if (message is Protocol.Request<Protocol.Continue> cont)
            {
                bool found = this.threads.TryGetValue(cont.arguments.threadId, out var thread);
                if (found)
                {
                    thread.Run.Post(Phase.Continue);
                }

                return Protocol.Response.From(NextSeq, cont, new { allThreadsContinued = false });
            }
            else if (message is Protocol.Request<Protocol.Pause> pause)
            {
                bool found = this.threads.TryGetValue(pause.arguments.threadId, out var thread);
                if (found)
                {
                    thread.Run.Post(Phase.Pause);
                }

                return Protocol.Response.From(NextSeq, pause, new { });
            }
            else if (message is Protocol.Request<Protocol.Next> next)
            {
                bool found = this.threads.TryGetValue(next.arguments.threadId, out var thread);
                if (found)
                {
                    thread.Run.Post(Phase.Next);
                }

                return Protocol.Response.From(NextSeq, next, new { });
            }
            else if (message is Protocol.Request<Protocol.Disconnect> disconnect)
            {
                // possibly run all threads

                return Protocol.Response.From(NextSeq, disconnect, new { });
            }
            else if (message is Protocol.Request request)
            {
                return Protocol.Response.From(NextSeq, request, new { });
            }
            else if (message is Protocol.Event @event)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected override async Task AcceptAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var token = await ReadAsync(cancellationToken).ConfigureAwait(false);
                    var message = Protocol.Parse(token);
                    var response = await DispatchAsync(message, cancellationToken).ConfigureAwait(false);
                    await SendAsync(response, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception error)
                {
                    this.logger.LogError(error, error.Message);
                    throw;
                }
            }
        }
    }
}
