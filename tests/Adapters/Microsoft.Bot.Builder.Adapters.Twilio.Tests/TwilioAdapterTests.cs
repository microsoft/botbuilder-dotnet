using System;
using Microsoft.Bot.Builder.Adapters.Twilio;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Twilio.Tests
{
    public class TwilioAdapterTests
    {
        [Fact]
        public void Constructor_Should_Fail_With_Null_Options()
        {
            Assert.Throws<ArgumentNullException>(() => { new TwilioAdapter(null); });
        }
    }
}
