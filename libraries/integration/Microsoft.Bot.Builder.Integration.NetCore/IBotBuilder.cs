using Microsoft.Bot.Builder.Middleware;

namespace Microsoft.Bot.Builder.Integration.NetCore
{
    public interface IBotFraweworkConfigurationBuilder
    {
        IBotFraweworkConfigurationBuilder UseApplicationIdentity(string applicationId, string applicationPassword);
        IBotFraweworkConfigurationBuilder UseMiddleware(IMiddleware middleware);
    }
}
