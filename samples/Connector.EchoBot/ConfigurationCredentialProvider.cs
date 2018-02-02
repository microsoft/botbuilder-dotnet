using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;

namespace Connector.EchoBot
{
    /// <summary>
    /// Credential provider which uses <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> to lookup appId and password.
    /// </summary>
    /// <remarks>
    /// This class is NOT included in the Connector package to avoid those packages needing to have a dependency
    /// on the ASP.Net Dependency Injection framework. 
    /// </remarks>
    public sealed class ConfigurationCredentialProvider : SimpleCredentialProvider
    {
        public ConfigurationCredentialProvider(IConfiguration configuration)
        {
            this.AppId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            this.Password = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value;
        }
    }
}
