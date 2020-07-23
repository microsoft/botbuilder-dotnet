// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class IActivityExtensionsTests
    {
        [Fact]
        public void SetAndGetLocaleOnConversationUpdate()
        {
            var sut = Activity.CreateConversationUpdateActivity();

            Assert.Null(sut.GetLocale());
            sut.SetLocale("en-UK");
            Assert.Equal("en-UK", sut.GetLocale());
        }
    }
}
