// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class OAuthCardTests
    {
        [Fact]
        public void OAuthCardInits()
        {
            var text = "I am an OAuthCard";
            var connectionName = "myConnectionName";
            var buttons = new List<CardAction>() { new CardAction("signin"), new CardAction("signout") };
            var tokenExchangeResource = new TokenExchangeResource("id", "http://example.com", "providerId");

            var oAuthCard = new OAuthCard(text, connectionName, buttons)
            {
                TokenExchangeResource = tokenExchangeResource
            };

            Assert.NotNull(oAuthCard);
            Assert.IsType<OAuthCard>(oAuthCard);
            Assert.Equal(text, oAuthCard.Text);
            Assert.Equal(connectionName, oAuthCard.ConnectionName);
            Assert.Equal(buttons, oAuthCard.Buttons);
            Assert.Equal(tokenExchangeResource, oAuthCard.TokenExchangeResource);
        }
        
        [Fact]
        public void OAuthCardInitsWithNoArgs()
        {
            var oAuthCard = new OAuthCard();

            Assert.NotNull(oAuthCard);
            Assert.IsType<OAuthCard>(oAuthCard);
        }
    }
}
