// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    // https://github.com/Microsoft/debug-adapter-protocol/blob/gh-pages/debugAdapterProtocol.json
    public static class Protocol
    {
        public static Request Parse(JToken token)
        {
            switch ((string)token["type"])
            {
                case "request":
                    switch ((string)token["command"])
                    {
                        case "launch": return token.ToObject<Request<Launch>>();
                        case "attach": return token.ToObject<Request<Attach>>();
                        case "initialize": return token.ToObject<Request<Initialize>>();
                        case "setBreakpoints": return token.ToObject<Request<SetBreakpoints>>();
                        case "setFunctionBreakpoints": return token.ToObject<Request<SetFunctionBreakpoints>>();
                        case "setExceptionBreakpoints": return token.ToObject<Request<SetExceptionBreakpoints>>();
                        case "configurationDone": return token.ToObject<Request<ConfigurationDone>>();
                        case "threads": return token.ToObject<Request<Threads>>();
                        case "stackTrace": return token.ToObject<Request<StackTrace>>();
                        case "scopes": return token.ToObject<Request<Scopes>>();
                        case "variables": return token.ToObject<Request<Variables>>();
                        case "setVariable": return token.ToObject<Request<SetVariable>>();
                        case "evaluate": return token.ToObject<Request<Evaluate>>();
                        case "continue": return token.ToObject<Request<Continue>>();
                        case "pause": return token.ToObject<Request<Pause>>();
                        case "next": return token.ToObject<Request<Next>>();
                        case "stepIn": return token.ToObject<Request<Next>>();
                        case "stepOut": return token.ToObject<Request<Next>>();
                        case "terminate": return token.ToObject<Request<Terminate>>();
                        case "disconnect": return token.ToObject<Request<Disconnect>>();
                        default: return token.ToObject<Request>();
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public abstract class Message
        {
            public int Seq { get; set; }

            public string Type { get; set; }

            [JsonExtensionData]
            public JObject Rest { get; set; }
        }

        public class Request : Message
        {
            public string Command { get; set; }

            public override string ToString() => Command;
        }

        public class Request<TArguments> : Request
        {
            public TArguments Arguments { get; set; }
        }

        public class LaunchAttach
        {
            public bool BreakOnStart { get; set; } = false;
        }

        public class Attach : LaunchAttach
        {
        }

        public class Launch : LaunchAttach
        {
        }

        public class Initialize
        {
            public string ClientID { get; set; }

            public string ClientName { get; set; }

            public string AdapterID { get; set; }

            public string PathFormat { get; set; }

            public bool LinesStartAt1 { get; set; }

            public bool ColumnsStartAt1 { get; set; }

            public bool SupportsVariableType { get; set; }

            public bool SupportsVariablePaging { get; set; }

            public bool SupportsRunInTerminalRequest { get; set; }

            public string Locale { get; set; }
        }

        public class SetBreakpoints
        {
            public Source Source { get; set; }

            public SourceBreakpoint[] Breakpoints { get; set; }

            public bool SourceModified { get; set; }
        }

        public class SetFunctionBreakpoints
        {
            public FunctionBreakpoint[] Breakpoints { get; set; }
        }

        public class SetExceptionBreakpoints
        {
            public string[] Filters { get; set; }
        }

        public class Threads
        {
        }

        public class Capabilities
        {
            public bool SupportsConfigurationDoneRequest { get; set; }

            public bool SupportsSetVariable { get; set; }

            public bool SupportsEvaluateForHovers { get; set; }

            public bool SupportsFunctionBreakpoints { get; set; }

            public ExceptionBreakpointFilter[] ExceptionBreakpointFilters { get; set; }

            public bool SupportTerminateDebuggee { get; set; }

            public bool SupportsTerminateRequest { get; set; }
        }

        public class ExceptionBreakpointFilter
        {
            public string Filter { get; set; }

            public string Label { get; set; }

            public bool Default { get; set; }
        }

        public abstract class PerThread
        {
            public ulong ThreadId { get; set; }
        }

        public class StackTrace : PerThread
        {
            public int? StartFrame { get; set; }

            public int? Levels { get; set; }
        }

        public class Continue : PerThread
        {
        }

        public class Pause : PerThread
        {
        }

        public class Next : PerThread
        {
        }

        public class Scopes
        {
            public ulong FrameId { get; set; }
        }

        public class Variables
        {
            public ulong VariablesReference { get; set; }
        }

        public class SetVariable
        {
            public ulong VariablesReference { get; set; }

            public string Name { get; set; }

            public string Value { get; set; }
        }

        public class Evaluate
        {
            public ulong FrameId { get; set; }

            public string Expression { get; set; }
        }

        public class ConfigurationDone
        {
        }

        public class Disconnect
        {
            public bool Restart { get; set; }

            public bool TerminateDebuggee { get; set; }
        }

        public class Terminate
        {
            public bool Restart { get; set; }
        }

        public class Event : Message
        {
#pragma warning disable SA1300 // Should begin with an uppercase letter.
            public Event(int seq, string @event)
            {
                this.Seq = seq;
                this.Type = "event";
                this.@event = @event;
            }

            public string @event { get; set; }
#pragma warning restore SA1300 // Should begin with an uppercase letter.

            public static Event<TBody> From<TBody>(int seq, string @event, TBody body) => new Event<TBody>(seq, @event) { Body = body };
        }

        public class Event<TBody> : Event
        {
            public Event(int seq, string @event)
                : base(seq, @event)
            {
            }

            public TBody Body { get; set; }
        }

        public class Response : Message
        {
            public Response(int seq, Request request)
            {
                this.Seq = seq;
                this.Type = "response";
                this.Request_seq = request.Seq;
                this.Success = true;
                this.Command = request.Command;
            }

            public int Request_seq { get; set; }

            public bool Success { get; set; }

            public string Message { get; set; }

            public string Command { get; set; }

            public static Response<TBody> From<TBody>(int seq, Request request, TBody body) => new Response<TBody>(seq, request) { Body = body };

            public static Response<string> Fail(int seq, Request request, string message) => new Response<string>(seq, request) { Body = message, Message = message, Success = false };
        }

        public class Response<TBody> : Response
        {
            public Response(int seq, Request request)
                : base(seq, request)
            {
            }

            public TBody Body { get; set; }
        }

        public abstract class Reference
        {
            public ulong Id { get; set; }
        }

        public class Range : Reference
        {
            public string Item { get; set; }

            public string More { get; set; }

            public Source Source { get; set; }

            public int? Line { get; set; }

            public int? Column { get; set; }

            public int? EndLine { get; set; }

            public int? EndColumn { get; set; }
        }

        public class Breakpoint : Range
        {
            public bool Verified { get; set; }

            public string Message { get; set; }
        }

        public class StackFrame : Range
        {
            public string Name { get; set; }
        }

        public sealed class Thread : Reference
        {
            public string Name { get; set; }
        }

        public sealed class Source
        {
            public Source(string path)
            {
                this.Name = System.IO.Path.GetFileName(path);
                this.Path = path;
            }

            public string Name { get; set; }

            public string Path { get; set; }
        }

        public sealed class SourceBreakpoint
        {
            public int Line { get; set; }
        }

        public sealed class FunctionBreakpoint
        {
            public string Name { get; set; }
        }
    }
}
