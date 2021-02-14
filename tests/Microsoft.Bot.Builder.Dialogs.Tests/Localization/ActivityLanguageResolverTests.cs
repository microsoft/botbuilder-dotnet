// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Localization;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class ActivityLanguageResolverTests
    {
        public static IEnumerable<object[]> ActivityLanguageResolverTestData()
        {
            yield return new object[] { string.Empty, Thread.CurrentThread.CurrentCulture.Name };
            yield return new object[] { null, Thread.CurrentThread.CurrentCulture.Name };
            yield return new object[] { "invalid-locale", Thread.CurrentThread.CurrentCulture.Name };
            yield return new object[] { "en-us", "en-us" };
            yield return new object[] { "EN-US", "EN-US" };
            yield return new object[] { "En-uS", "En-uS" };
            yield return new object[] { "en", "en" };
            yield return new object[] { "es-ar", "es-ar" };
        }

        [Theory]
        [MemberData(nameof(ActivityLanguageResolverTestData))]

        public void ActivityLanguageResolution(string activityLocale, string expectedLocale)
        {
            // Arrange
            if (expectedLocale == null)
            {
                expectedLocale = activityLocale;
            }

            var dc = BuildTestDialogContext(activityLocale);
            var resolver = new ActivityLocaleResolver();

            // Act
            var computedLocale = resolver.Resolve(dc);

            // Assert
            Assert.NotNull(computedLocale);
            Assert.Equal(new CultureInfo(expectedLocale).Name, computedLocale.Name);
        }

        private static DialogContext BuildTestDialogContext(string activityLocale)
        {
            var turnContext = new TurnContext(new TestAdapter(), new Activity() { Type = ActivityTypes.Message, Locale = activityLocale });
            return new DialogContext(new DialogSet(), turnContext, new DialogState());
        }
    }
}
