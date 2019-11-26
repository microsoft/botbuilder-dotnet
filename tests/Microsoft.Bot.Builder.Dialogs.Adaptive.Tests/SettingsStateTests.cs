// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class SettingsStateTests
    {
        public SettingsStateTests()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task SettingsStateTests_SettingsTest()
        {
            await TestUtils.RunTestScript("SettingsStateTests_SettingsTest.test.dialog", configuration: Configuration);
        }

        [TestMethod]
        public async Task SettingsStateTests_TestTurnStateAcrossBoundaries()
        {
            await TestUtils.RunTestScript("SettingsStateTests_TestTurnStateAcrossBoundaries.test.dialog", configuration: Configuration);
        }
    }
}
