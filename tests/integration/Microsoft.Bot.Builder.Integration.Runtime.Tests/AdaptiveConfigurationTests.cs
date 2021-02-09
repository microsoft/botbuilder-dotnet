// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests
{
    public class AdaptiveConfigurationTests
    {
        private const int Max = 6;
        private const int Min = 1;
        private const string PathOne = "one";
        private const string PathTwo = "two";
        private const string PathThree = "three";
        private const char Separator = ':';

        private static readonly Random Random = new Random();

        public static IEnumerable<object[]> GetAdaptiveConfigurationTryGetValueData()
        {
            string value = Guid.NewGuid().ToString();

            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot(new JObject
            {
                {
                    PathOne, new JObject
                    {
                        {
                            PathTwo, new JObject
                            {
                                { PathThree, value }
                            }
                        }
                    }
                }
            });

            var separators = new char[] { '_', '.', ':' };
            foreach (char leftSeparator in separators)
            {
                foreach (char rightSeparator in separators)
                {
                    yield return new object[]
                    {
                        configuration,
                        BuildPath(leftSeparator, rightSeparator),
                        true,
                        value
                    };
                }
            }

            yield return new object[]
            {
                configuration,
                "missingPath",
                false,
                (object)null
            };
        }

        [Theory]
        [InlineData((string)null, null)]
        [InlineData("", null)]
        [InlineData("path", null)]
        [InlineData("path", "value")]
        public void AdaptiveConfiguration_SetValue(string path, object value)
        {
            var configuration = new ConfigurationMemory(TestDataGenerator.BuildConfigurationRoot());
            Assert.Throws<InvalidOperationException>(() => configuration.SetValue(path, value));
        }

        [Fact]
        public void AdaptiveConfiguration_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                "configuration",
                () => new ConfigurationMemory(null));
        }

        [Theory]
        [MemberData(nameof(GetAdaptiveConfigurationTryGetValueData))]
        public void AdaptiveConfiguration_TryGetValue(
            IConfiguration configuration,
            string path,
            bool expectedResult,
            object expectedValue)
        {
            bool result = new ConfigurationMemory(configuration).TryGetValue(path, out object value);
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void AdaptiveConfiguration_TryGetValue_Throws_ArgumentNullException()
        {
            var configuration = new ConfigurationMemory(TestDataGenerator.BuildConfigurationRoot());
            Assert.Throws<ArgumentNullException>(
                "path",
                () => configuration.TryGetValue(path: null, out object value));
        }

        [Fact]
        public void AdaptiveConfiguration_Version()
        {
            var configuration = new ConfigurationMemory(TestDataGenerator.BuildConfigurationRoot());
            Assert.Equal("1", configuration.Version());
        }

        private static string BuildPath(char leftSeparator, char rightSeparator)
        {
            return string.Concat(
                PathOne,
                new string(leftSeparator, leftSeparator == Separator ? 1 : Random.Next(Min, Max)),
                PathTwo,
                new string(rightSeparator, rightSeparator == Separator ? 1 : Random.Next(Min, Max)),
                PathThree);
        }
    }
}
