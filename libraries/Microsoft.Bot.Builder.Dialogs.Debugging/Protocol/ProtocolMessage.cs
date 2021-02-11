// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    // https://github.com/Microsoft/debug-adapter-protocol/blob/gh-pages/debugAdapterProtocol.json
    internal static class ProtocolMessage
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Include,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static JToken ToToken(Message message)
        {
            var token = JToken.FromObject(message, Serializer);
            return token;
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
    }
}
