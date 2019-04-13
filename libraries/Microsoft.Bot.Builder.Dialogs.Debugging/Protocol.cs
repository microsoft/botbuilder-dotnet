using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    // https://github.com/Microsoft/debug-adapter-protocol/blob/gh-pages/debugAdapterProtocol.json
    public static class Protocol
    {
        public abstract class Message
        {
            public int seq { get; set; }
            public string type { get; set; }
            [JsonExtensionData]
            public JObject Rest { get; set; }
        }

        public class Request : Message
        {
            public string command { get; set; }
            public override string ToString() => command;
        }

        public class Request<Arguments> : Request
        {
            public Arguments arguments { get; set; }
        }
        public class Attach
        {
        }
        public class Launch
        {
        }
        public class Initialize
        {
            public string clientID { get; set; }
            public string clientName { get; set; }
            public string adapterID { get; set; }
            public string pathFormat { get; set; }
            public bool linesStartAt1 { get; set; }
            public bool columnsStartAt1 { get; set; }
            public bool supportsVariableType { get; set; }
            public bool supportsVariablePaging { get; set; }
            public bool supportsRunInTerminalRequest { get; set; }
            public string locale { get; set; }
        }

        public class SetBreakpoints
        {
            public Source source { get; set; }
            public SourceBreakpoint[] breakpoints { get; set; }
            public bool sourceModified { get; set; }
        }
        public class Threads
        {
        }
        public abstract class PerThread
        {
            public int threadId { get; set; }
        }
        public class StackTrace : PerThread
        {
            public int? startFrame { get; set; }
            public int? levels { get; set; }
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
            public int frameId { get; set; }
        }
        public class Variables
        {
            public int variablesReference { get; set; }
        }
        public class SetVariable
        {
            public int variablesReference { get; set; }
            public string name { get; set; }
            public string value { get; set; }
        }
        public class Evaluate
        {
            public int frameId { get; set; }
            public string expression { get; set; }
        }
        public class ConfigurationDone
        {
        }
        public class Disconnect
        {
            public bool restart { get; set; }
            public bool terminateDebuggee { get; set; }
        }
        public class Event : Message
        {
            public Event(int seq, string @event)
            {
                this.seq = seq;
                this.type = "event";
                this.@event = @event;
            }
            public string @event { get; set; }
            public static Event<Body> From<Body>(int seq, string @event, Body body) => new Event<Body>(seq, @event) { body = body };
        }
        public class Event<Body> : Event
        {
            public Event(int seq, string @event)
                : base(seq, @event)
            {
            }
            public Body body { get; set; }
        }

        public class Response : Message
        {
            public Response(int seq, Request request)
            {
                this.seq = seq;
                this.type = "response";
                this.request_seq = request.seq;
                this.success = true;
                this.command = request.command;
            }
            public int request_seq { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
            public string command { get; set; }
            public static Response<Body> From<Body>(int seq, Request request, Body body) => new Response<Body>(seq, request) { body = body };
            public static Response<string> Fail(int seq, Request request, string message) => new Response<string>(seq, request) { body = message, message = message, success = false };
        }

        public class Response<Body> : Response
        {
            public Response(int seq, Request request)
                : base(seq, request)
            {
            }
            public Body body { get; set; }
        }

        public abstract class Reference
        {
            public int id { get; set; }
        }

        public class Range : Reference
        {
            public Source source { get; set; }
            public int? line { get; set; }
            public int? column { get; set; }
            public int? endLine { get; set; }
            public int? endColumn { get; set; }
        }

        public class Breakpoint : Range
        {
            public bool verified { get; set; }
            public string message { get; set; }
        }

        public class StackFrame : Range
        {
            public string name { get; set; }
        }

        public sealed class Thread : Reference
        {
            public string name { get; set; }
        }

        public sealed class Source
        {
            public Source(string path)
            {
                this.name = Path.GetFileName(path);
                this.path = path;
            }

            public string name { get; set; }
            public string path { get; set; }
        }
        public sealed class SourceBreakpoint
        {
            public int line { get; set; }
        }

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
                        case "configurationDone": return token.ToObject<Request<ConfigurationDone>>();
                        case "disconnect": return token.ToObject<Request<Disconnect>>();
                        case "setFunctionBreakpoints":
                        case "setExceptionBreakpoints":
                        default: return token.ToObject<Request>();
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
