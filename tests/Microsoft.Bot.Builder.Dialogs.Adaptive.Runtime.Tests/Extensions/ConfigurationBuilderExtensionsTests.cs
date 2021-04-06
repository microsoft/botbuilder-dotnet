// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Tests.Extensions
{
    public class ConfigurationBuilderExtensionsTests
    {
        [Fact]
        public void ParameterValidation()
        {
            var sut = new ConfigurationBuilder();
            Assert.Throws<ArgumentNullException>(() =>
            {
                sut.AddBotRuntimeConfiguration(null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                ConfigurationBuilderExtensions.AddBotRuntimeConfiguration(null, "blah");
            });
        }

        [Theory]
        [InlineData(null, "NoEnvironmentValue")]
        [InlineData("Development", "DevEnvironmentValue")]
        [InlineData("Test", "TestEnvironmentValue")]
        [InlineData("NotThere", "NoEnvironmentValue")]
        public void LoadsAppSettings(string environmentName, string expectedSetting)
        {
            // Arrange
            var applicationRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions", "ConfigurationBuilderExtensionsFiles");

            // Act
            var sut = new ConfigurationBuilder();
            sut.AddBotRuntimeConfiguration(applicationRoot, "settings", environmentName);
            var actualConfig = sut.Build();

            // Assert runtime properties.
            Assert.Equal(applicationRoot, actualConfig["applicationRoot"]);
            Assert.Equal("RootDialog.dialog", actualConfig["defaultRootDialog"]);

            // Assert appsettings values are loaded based on EnvironmentName.
            Assert.Equal(expectedSetting, actualConfig["testSetting"]);
        }
    }
}
