// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class SignInTests
    {
        [Fact]
        public void SignInResourceInits()
        {
            var signInLink = "http://example-signin-link.com";
            var tokenExchangeResource = new TokenExchangeResource("id", "uri", "providerId");

            var signInResource = new SignInResource(signInLink, tokenExchangeResource);

            Assert.NotNull(signInResource);
            Assert.IsType<SignInResource>(signInResource);
            Assert.Equal(signInLink, signInResource.SignInLink);
            Assert.Equal(tokenExchangeResource, signInResource.TokenExchangeResource);
        }

        [Fact]
        public void SignInResourceInitsWithNoArgs()
        {
            var signInResource = new SignInResource();

            Assert.NotNull(signInResource);
            Assert.IsType<SignInResource>(signInResource);
        }

        [Fact]
        public void SignInCardInits()
        {
            var text = "Please sign in.";
            var buttons = new List<CardAction>() { new CardAction("signin") };

            var signInCard = new SigninCard(text, buttons);

            Assert.NotNull(signInCard);
            Assert.IsType<SigninCard>(signInCard);
            Assert.Equal(text, signInCard.Text);
            Assert.Equal(buttons, signInCard.Buttons);
        }
        
        [Fact]
        public void SignInCardInitsWithNoArgs()
        {
            var signInCard = new SigninCard();

            Assert.NotNull(signInCard);
            Assert.IsType<SigninCard>(signInCard);
        }

        [Fact]
        public void SignInCardCreate()
        {
            var signInCard = SigninCard.Create("Please sign in", "Sign In", "http://example-signin.com");

            Assert.NotNull(signInCard);
            Assert.IsType<SigninCard>(signInCard);
        }
    }
}
