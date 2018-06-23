// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Bot.Connector.Authentication;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class EndorsementsValidatorTests
    {
        [Fact]
        public void NullChannelIdParameterShouldPass()
        {
            var isEndorsed = EndorsementsValidator.Validate(null, new HashSet<string>());
            isEndorsed.Should().BeTrue();
        }

        [Fact]
        public void NullEndorsementsParameterShouldThrow()
        {
            Action action = () => EndorsementsValidator.Validate("foo", null);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("endorsements");
        }

        [Fact]
        public void UnendorsedChannelIdShouldFail()
        {
            var isEndorsed = EndorsementsValidator.Validate("channelOne", new HashSet<string>());
            isEndorsed.Should().BeFalse();
        }

        [Fact]
        public void MismatchedEndorsementsChannelIdShouldFail()
        {
            var isEndorsed = EndorsementsValidator.Validate("right", new HashSet<string>(new[] { "wrong" }));
            isEndorsed.Should().BeFalse();
        }

        [Fact]
        public void EndorsedChannelIdShouldPass()
        {
            var isEndorsed = EndorsementsValidator.Validate("right", new HashSet<string>(new[] { "right" }));
            isEndorsed.Should().BeTrue();
        }

        [Fact]
        public void EndorsedChannelIdShouldPassWithTwoEndorsements()
        {
            var isEndorsed = EndorsementsValidator.Validate("right", new HashSet<string>(new [] { "right", "wrong" }));
            isEndorsed.Should().BeTrue();
        }

        [Fact]
        public void UnaffinitizedActivityShouldPass()
        {
            var isEndorsed = EndorsementsValidator.Validate(string.Empty, new HashSet<string>(new[] { "right", "wrong" }));
            isEndorsed.Should().BeTrue();
        }
    }
}