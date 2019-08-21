using System;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Webex.Tests
{
    public class WebexAdapterTests
    {
        [Fact]
        public void Constructor_Should_Fail_With_Null_Config()
        {
            Assert.Throws<ArgumentNullException>(() => { new WebexAdapter(null); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_AccessToken()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = null;
            options.Object.PublicAddress = "Test";

            Assert.Throws<Exception>(() => { new WebexAdapter(options.Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_PublicAddress()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = null;

            Assert.Throws<Exception>(() => { new WebexAdapter(options.Object); });
        }
    }
}
