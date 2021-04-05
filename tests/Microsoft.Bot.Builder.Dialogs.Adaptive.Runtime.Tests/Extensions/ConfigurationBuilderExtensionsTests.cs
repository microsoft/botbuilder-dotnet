// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
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
                sut.AddBotRuntimeConfiguration(null, "blah");
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                sut.AddBotRuntimeConfiguration(new HostBuilderContext(new Dictionary<object, object>()), null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                sut.AddBotRuntimeConfiguration(new HostBuilderContext(new Dictionary<object, object>()), string.Empty);
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
            var mockBuilderContext = new HostBuilderContext(new Dictionary<object, object>())
            {
                HostingEnvironment = new TestHostingEnvironment { EnvironmentName = environmentName }
            };
            var applicationRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions", "ConfigurationBuilderExtensionsFiles");

            // Act
            var sut = new ConfigurationBuilder();
            sut.AddBotRuntimeConfiguration(mockBuilderContext, applicationRoot, "settings");
            var actualConfig = sut.Build();

            // Assert runtime properties.
            Assert.Equal(applicationRoot, actualConfig["applicationRoot"]);
            Assert.Equal("RootDialog.dialog", actualConfig["defaultRootDialog"]);

            // Assert appsettings values are loaded based on EnvironmentName.
            Assert.Equal(expectedSetting, actualConfig["testSetting"]);
        }

#if NETCOREAPP2_1
        /// <summary>
        /// Help implementation of <see cref="Microsoft.Extensions.Hosting.IHostEnvironment"/> use for testing.
        /// </summary>
        private class TestHostingEnvironment : Microsoft.Extensions.Hosting.IHostingEnvironment
#else
        private class TestHostingEnvironment : IHostEnvironment
#endif
        {
            public string EnvironmentName { get; set; }

            public string ApplicationName { get; set; }

            public string ContentRootPath { get; set; }

            public IFileProvider ContentRootFileProvider { get; set; }
        }
    }
}
