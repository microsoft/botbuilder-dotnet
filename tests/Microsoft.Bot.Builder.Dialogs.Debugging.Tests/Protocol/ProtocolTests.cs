// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Debugging.Protocol;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.Protocol
{
    public sealed class ProtocolTests
    {
        [Fact]
        public void BreakPoint_Get_Set()
        {
            var breakPoint = new Breakpoint
            {
                Message = "test message",
                Verified = true
            };

            Assert.Equal("test message", breakPoint.Message);
            Assert.True(breakPoint.Verified);
        }

        [Fact]
        public void Disconnect_Get_Set()
        {
            var disconnect = new Disconnect
            {
                Restart = true,
                TerminateDebuggee = false
            };

            Assert.True(disconnect.Restart);
            Assert.False(disconnect.TerminateDebuggee);
        }

        [Fact]
        public void Evaluate_Get_Set()
        {
            var evaluate = new Evaluate
            {
                FrameId = 0ul,
                Expression = "test expression"
            };

            Assert.Equal(0ul, evaluate.FrameId);
            Assert.Equal("test expression", evaluate.Expression);
        }

        [Fact]
        public void ProtocolMessage_SwitchCase()
        {
            var writer = new JTokenWriter();
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("request");
            writer.WritePropertyName("command");
            writer.WriteValue("launch");
            writer.WriteEndObject();

            var token = writer.Token;
            var request = ProtocolMessage.Parse(token);
            Assert.Equal("launch", request.Command);

            token["command"] = "setBreakpoints";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("setBreakpoints", request.Command);

            token["command"] = "setFunctionBreakpoints";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("setFunctionBreakpoints", request.Command);

            token["command"] = "setExceptionBreakpoints";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("setExceptionBreakpoints", request.Command);

            token["command"] = "configurationDone";
            request = Debugging.Protocol.ProtocolMessage.Parse(token);
            Assert.Equal("configurationDone", request.Command);

            token["command"] = "stackTrace";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("stackTrace", request.Command);

            token["command"] = "scopes";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("scopes", request.Command);

            token["command"] = "variables";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("variables", request.Command);

            token["command"] = "setVariable";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("setVariable", request.Command);

            token["command"] = "evaluate";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("evaluate", request.Command);

            token["command"] = "continue";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("continue", request.Command);

            token["command"] = "pause";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("pause", request.Command);

            token["command"] = "stepIn";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("stepIn", request.Command);

            token["command"] = "stepOut";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("stepOut", request.Command);

            token["command"] = "terminate";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("terminate", request.Command);

            token["command"] = "disconnect";
            request = ProtocolMessage.Parse(token);
            Assert.Equal("disconnect", request.Command);

            token["type"] = "other";
            Assert.Throws<NotImplementedException>(() =>
                ProtocolMessage.Parse(token));
        }

        [Fact]
        public void Request_ToString()
        {
            var request = new Request { Command = "test-command" };

            Assert.Equal("test-command", request.Command);
            Assert.Equal("test-command", request.ToString());
        }

        [Fact]
        public void Response_Get_Set()
        {
            var response = new Response(2, new Request())
            {
                Message = "test-message"
            };

            Assert.Equal("test-message", response.Message);
        }

        [Fact]
        public void Response_Fail()
        {
            var result = Response.Fail(3, new Request(), "fail message");
            Assert.Equal("fail message", result.Message);
            Assert.Equal("fail message", result.Body);
            Assert.False(result.Success);
        }

        [Fact]
        public void Scopes_Get_Set()
        {
            var scopes = new Scopes { FrameId = 0ul };

            Assert.Equal(0ul, scopes.FrameId);
        }

        [Fact]
        public void SetBreakpoints_Get_Set()
        {
            var setBreakpoints = new SetBreakpoints
            {
                Source = new Source("/test/path"),
                Breakpoints = new[] { new SourceBreakpoint() },
                SourceModified = false
            };

            Assert.Equal("/test/path", setBreakpoints.Source.Path);
            Assert.Single(setBreakpoints.Breakpoints);
            Assert.False(setBreakpoints.SourceModified);
        }

        [Fact]
        public void SetExceptionBreakpoints_Get_Set()
        {
            var setBreakpoints = new SetExceptionBreakpoints { Filters = new[] { "testFilter" } };

            Assert.Single(setBreakpoints.Filters);
            Assert.Equal("testFilter", setBreakpoints.Filters[0]);
        }

        [Fact]
        public void SetFunctionBreakpoints_Get_Set()
        {
            var setBreakpoints = new SetFunctionBreakpoints { Breakpoints = new[] { new FunctionBreakpoint { Name = "test-breakpoint" } } };

            Assert.Single(setBreakpoints.Breakpoints);
            Assert.Equal("test-breakpoint", setBreakpoints.Breakpoints[0].Name);
        }

        [Fact]
        public void SetVariable_Get_Set()
        {
            var variable = new SetVariable
            {
                VariablesReference = 0ul,
                Name = "test-variable",
                Value = "value"
            };

            Assert.Equal(0ul, variable.VariablesReference);
            Assert.Equal("test-variable", variable.Name);
            Assert.Equal("value", variable.Value);
        }

        [Fact]
        public void Source_Get_Set()
        {
            var source = new Source("/test/path")
            {
                Name = "test-source"
            };

            Assert.Equal("/test/path", source.Path);
            Assert.Equal("test-source", source.Name);
        }

        [Fact]
        public void StackTrace_Get_Set()
        {
            var stackTrace = new StackTrace
            {
                StartFrame = 1,
                Levels = 2
            };

            Assert.Equal(1, stackTrace.StartFrame);
            Assert.Equal(2, stackTrace.Levels);
        }

        [Fact]
        public void Terminate_Get_Set()
        {
            var terminate = new Terminate { Restart = true };

            Assert.True(terminate.Restart);
        }

        [Fact]
        public void Thread_Get_Set()
        {
            var thread = new Thread { Name = "test-thread" };

            Assert.Equal("test-thread", thread.Name);
        }

        [Fact]
        public void Variables_Get_Set()
        {
            var variables = new Variables { VariablesReference = 0ul };

            Assert.Equal(0ul, variables.VariablesReference);
        }
    }
}
