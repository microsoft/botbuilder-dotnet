// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class SuggestedActionsTests
    {
        [Fact]
        public void SuggestedActionsInits()
        {
            var to = new List<string>() { "recipient1" };
            var actions = new List<CardAction>() { new CardAction() };

            var suggestedAction = new SuggestedActions(to, actions);

            Assert.NotNull(suggestedAction);
            Assert.IsType<SuggestedActions>(suggestedAction);
            Assert.Equal(to, suggestedAction.To);
            Assert.Equal(actions, suggestedAction.Actions);
        }
        
        [Fact]
        public void SuggestedActionsInitsWithNoArgs()
        {
            var suggestedAction = new SuggestedActions();

            Assert.NotNull(suggestedAction);
            Assert.IsType<SuggestedActions>(suggestedAction);
        }

        [Fact]
        public void SuggestedActionsInitsWithIEnumerables()
        {
            var to = new SuggestedActionEnumerableTo();
            var actions = new SuggestedActionEnumerableActions();

            var suggestedActions = new SuggestedActions(to, actions);

            Assert.NotNull(suggestedActions);
            Assert.IsType<SuggestedActions>(suggestedActions);
        }

        private class SuggestedActionEnumerableTo : IEnumerable<string>
        {
            public IEnumerator<string> GetEnumerator()
            {
                yield return "test";
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        } 
        
        private class SuggestedActionEnumerableActions : IEnumerable<CardAction>
        {
            public IEnumerator<CardAction> GetEnumerator()
            {
                yield return new CardAction();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
