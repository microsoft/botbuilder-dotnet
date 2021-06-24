// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class AdaptiveCardTests
    {
        [Fact]
        public void AdaptiveCardAuthentication()
        {
            var id = "myId";
            var connectionName = "myConnectionName";
            var token = "mySpecialToken";

            var authSpecs = new AdaptiveCardAuthentication()
            {
                Id = id,
                ConnectionName = connectionName,
                Token = token,
            };

            Assert.Equal(id, authSpecs.Id);
            Assert.Equal(connectionName, authSpecs.ConnectionName);
            Assert.Equal(token, authSpecs.Token);
        }

        [Fact]
        public void AdaptiveCardInvokeAction()
        {
            var type = "myType";
            var id = "myId";
            var verb = "stupefy";
            var data = new { };

            var actionSpecs = new AdaptiveCardInvokeAction()
            {
                Type = type,
                Id = id,
                Verb = verb,
                Data = data
            };

            Assert.Equal(type, actionSpecs.Type);
            Assert.Equal(id, actionSpecs.Id);
            Assert.Equal(verb, actionSpecs.Verb);
            Assert.Equal(data, actionSpecs.Data);
        }

        [Fact]
        public void AdaptiveCardInvokeResponse()
        {
            var statusCode = 200;
            var type = "myType";
            var value = new { };

            var res = new AdaptiveCardInvokeResponse()
            {
                StatusCode = statusCode,
                Type = type,
                Value = value,
            };

            Assert.Equal(statusCode, res.StatusCode);
            Assert.Equal(type, res.Type);
            Assert.Equal(value, res.Value);
        }

        [Fact]
        public void AdaptiveCardInvokeValue()
        {
            var action = new AdaptiveCardInvokeAction()
            {
                Type = "myType",
                Id = "actionId",
                Verb = "stupefy",
                Data = new { }
            };
            var auth = new AdaptiveCardAuthentication()
            {
                Id = "authId",
                ConnectionName = "myConnectionName",
                Token = "mySpecialToken",
            };
            var state = "myState";

            var invokeValue = new AdaptiveCardInvokeValue()
            {
                Action = action,
                Authentication = auth,
                State = state,
            };

            Assert.Equal(action, invokeValue.Action);
            Assert.Equal(auth, invokeValue.Authentication);
            Assert.Equal(state, invokeValue.State);
        }
    }
}
