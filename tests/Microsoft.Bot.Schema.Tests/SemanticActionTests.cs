// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class SemanticActionTests
    {
        [Fact]
        public void SemanticActionInits()
        {
            var id = "id";
            var entities = new Dictionary<string, Entity>() { { "entityKey", new Entity() } };
            var state = "done";

            var semanticAction = new SemanticAction(id, entities)
            {
                State = state
            };

            Assert.NotNull(semanticAction);
            Assert.IsType<SemanticAction>(semanticAction);
            Assert.Equal(id, semanticAction.Id);
            Assert.Equal(entities, semanticAction.Entities);
            Assert.Equal(state, semanticAction.State);
        }

        [Fact]
        public void SemanticActionInitsWithNoArgs()
        {
            var semanticAction = new SemanticAction();

            Assert.NotNull(semanticAction);
            Assert.IsType<SemanticAction>(semanticAction);
        }
    }
}
