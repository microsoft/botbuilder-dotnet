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

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public abstract class Message
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public int Seq { get; set; }

            public string Type { get; set; }

            [JsonExtensionData]
            public JObject Rest { get; } = new JObject();
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Request : Message
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public string Command { get; set; }

            public override string ToString() => Command;
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Request<TArguments> : Request
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public TArguments Arguments { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class LaunchAttach
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public bool BreakOnStart { get; set; } = false;
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Attach : LaunchAttach
#pragma warning restore CA1034 // Nested types should not be visible
        {
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Launch : LaunchAttach
#pragma warning restore CA1034 // Nested types should not be visible
        {
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Initialize
#pragma warning restore CA1034 // Nested types should not be visible
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

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class SetBreakpoints
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public Source Source { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
            public SourceBreakpoint[] Breakpoints { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

            public bool SourceModified { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class SetFunctionBreakpoints
#pragma warning restore CA1034 // Nested types should not be visible
        {
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
            public FunctionBreakpoint[] Breakpoints { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class SetExceptionBreakpoints
#pragma warning restore CA1034 // Nested types should not be visible
        {
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
            public string[] Filters { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Threads
#pragma warning restore CA1034 // Nested types should not be visible
        {
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Capabilities
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public bool SupportsConfigurationDoneRequest { get; set; }

            public bool SupportsSetVariable { get; set; }

            public bool SupportsEvaluateForHovers { get; set; }

            public bool SupportsFunctionBreakpoints { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
            public ExceptionBreakpointFilter[] ExceptionBreakpointFilters { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

            public bool SupportTerminateDebuggee { get; set; }

            public bool SupportsTerminateRequest { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class ExceptionBreakpointFilter
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public string Filter { get; set; }

            public string Label { get; set; }

            public bool Default { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public abstract class PerThread
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public ulong ThreadId { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class StackTrace : PerThread
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public int? StartFrame { get; set; }

            public int? Levels { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and we can't change it without breaking binary compat)
        public class Continue : PerThread
#pragma warning restore CA1716 // Identifiers should not match keywords
#pragma warning restore CA1034 // Nested types should not be visible
        {
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Pause : PerThread
#pragma warning restore CA1034 // Nested types should not be visible
        {
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and we can't change it without breaking binary compat)
        public class Next : PerThread
#pragma warning restore CA1716 // Identifiers should not match keywords
#pragma warning restore CA1034 // Nested types should not be visible
        {
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
#pragma warning disable CA1724 // Type names should not match namespaces (by design and we can't change this without breaking binary compat)
        public class Scopes
#pragma warning restore CA1724 // Type names should not match namespaces
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public ulong FrameId { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Variables
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public ulong VariablesReference { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class SetVariable
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public ulong VariablesReference { get; set; }

            public string Name { get; set; }

            public string Value { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Evaluate
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public ulong FrameId { get; set; }

            public string Expression { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class ConfigurationDone
#pragma warning restore CA1034 // Nested types should not be visible
        {
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Disconnect
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public bool Restart { get; set; }

            public bool TerminateDebuggee { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Terminate
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public bool Restart { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and we can't change it without breaking binary compat)
        public class Event : Message
#pragma warning restore CA1716 // Identifiers should not match keywords
#pragma warning restore CA1034 // Nested types should not be visible
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

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and we can't change it without breaking binary compat)
        public class Event<TBody> : Event
#pragma warning restore CA1716 // Identifiers should not match keywords
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public Event(int seq, string @event)
                : base(seq, @event)
            {
            }

            public TBody Body { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Response : Message
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public Response(int seq, Request request)
            {
                this.Seq = seq;
                this.Type = "response";
                this.Request_seq = request.Seq;
                this.Success = true;
                this.Command = request.Command;
            }

#pragma warning disable CA1707 // Identifiers should not contain underscores (we can't change this without breaking binary compat)
            public int Request_seq { get; set; }
#pragma warning restore CA1707 // Identifiers should not contain underscores

            public bool Success { get; set; }

            public string Message { get; set; }

            public string Command { get; set; }

            public static Response<TBody> From<TBody>(int seq, Request request, TBody body) => new Response<TBody>(seq, request) { Body = body };

            public static Response<string> Fail(int seq, Request request, string message) => new Response<string>(seq, request) { Body = message, Message = message, Success = false };
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Response<TBody> : Response
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public Response(int seq, Request request)
                : base(seq, request)
            {
            }

            public TBody Body { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public abstract class Reference
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public ulong Id { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Range : Reference
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public string Item { get; set; }

            public string More { get; set; }

            public JToken Designer { get; set; }

            public Source Source { get; set; }

            public int? Line { get; set; }

            public int? Column { get; set; }

            public int? EndLine { get; set; }

            public int? EndColumn { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class Breakpoint : Range
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public bool Verified { get; set; }

            public string Message { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public class StackFrame : Range
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public string Name { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public sealed class Thread : Reference
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public string Name { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public sealed class Source
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public Source(string path)
            {
                this.Name = System.IO.Path.GetFileName(path);
                this.Path = path;
            }

            public string Name { get; set; }

            public string Path { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public sealed class SourceBreakpoint
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public int Line { get; set; }
        }

#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat, consider fixing this before we go out of preview)
        public sealed class FunctionBreakpoint
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public string Name { get; set; }
        }
    }
}
