// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    public class ExpressionPropertyExtensionTests
    {
        public static IEnumerable<object[]> GetConfigurationValueThrowsArgumentNullExceptionData()
        {
            yield return new object[]
            {
                "property",
                (ExpressionProperty<string>)null,
                TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                "configuration",
                new StringExpression(string.Empty),
                (IConfiguration)null
            };
        }

        [Theory]
        [MemberData(nameof(GetConfigurationValueThrowsArgumentNullExceptionData))]
        public void GetConfigurationValue_Throws_ArgumentNullException(
            string paramName,
            ExpressionProperty<string> property,
            IConfiguration configuration)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => property.GetConfigurationValue(configuration));
        }
    }
}
