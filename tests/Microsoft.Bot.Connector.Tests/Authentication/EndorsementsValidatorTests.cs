using System;
using FluentAssertions;
using Microsoft.Bot.Connector.Authentication;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class EndorsementsValidatorTests
    {
        [Fact]
        public void NullAddressParameterShouldPass()
        {
            bool isEndorsed = EndorsementsValidator.Validate(null, new string[] { });
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
            bool isEndorsed = EndorsementsValidator.Validate("channelOne", new string[] { });
            isEndorsed.Should().BeFalse();
        }

        [Fact]
        public void MismatchedEndorsementsChannelIdShouldFail()
        {
            bool isEndorsed = EndorsementsValidator.Validate("right", new[] { "wrong" });
            isEndorsed.Should().BeFalse();
        }

        [Fact]
        public void EndorsedChannelIdShouldPass()
        {
            bool isEndorsed = EndorsementsValidator.Validate("right", new[] { "right" });
            isEndorsed.Should().BeTrue();
        }

        [Fact]
        public void EndorsedChannelIdShouldPassWithTwoEndorsements()
        {
            bool isEndorsed = EndorsementsValidator.Validate("right", new[] { "right", "wrong" });
            isEndorsed.Should().BeTrue();
        }

        [Fact]
        public void UnaffinitizedActivityShouldPass()
        {
            bool isEndorsed = EndorsementsValidator.Validate(string.Empty, new[] { "right", "wrong" });
            isEndorsed.Should().BeTrue();
        }

        [Fact]
        public void UnendorsedChannelShouldPassDueToOverride()
        {
            bool isEndorsed = EndorsementsValidator.Validate("unendorsed", new[] { "wrong" }, true);
            isEndorsed.Should().BeTrue();
        }
    }
}