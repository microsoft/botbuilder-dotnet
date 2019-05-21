// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.Utils
{
    /// <summary>
    /// A base class with helper methods and properties to test dialogs in isolation.
    /// </summary>
    public class DialogTestsBase
    {
        public DialogTestsBase()
            : this(null)
        {
        }

        public DialogTestsBase(ITestOutputHelper output)
        {
            Output = output;
            MockLogger = new Mock<ILogger<MainDialog>>();
            MockConfig = new Mock<IConfiguration>();
            MockConfig.Setup(x => x["LuisAppId"]).Returns("SomeLuisAppId");
            MockConfig.Setup(x => x["LuisAPIKey"]).Returns("SomeLuisAppKey");
            MockConfig.Setup(x => x["LuisAPIHostName"]).Returns("SomeLuisAppHostName");
        }

        protected ITestOutputHelper Output { get; }

        protected Mock<IConfiguration> MockConfig { get; }

        protected Mock<ILogger<MainDialog>> MockLogger { get; }
    }
}
