using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Connector.Tests
{
    public class MicrosoftGovernmentAppCredentialsTests
    {
        [Fact]
        public void MicrosoftGovernmentAppCredentials_Has_Gov_Endpoint()
        {
            var cred = new MicrosoftGovernmentAppCredentials(string.Empty, string.Empty);

            Assert.Contains("login.microsoftonline.us", cred.OAuthEndpoint);
        }
        
        [Fact]
        public void MicrosoftGovernmentAppCredentials_Uses_Gov_Scope()
        {
            var cred = new MicrosoftGovernmentAppCredentials(string.Empty, string.Empty);

            Assert.Contains("api.botframework.us", cred.OAuthScope);
        }
    }
}
