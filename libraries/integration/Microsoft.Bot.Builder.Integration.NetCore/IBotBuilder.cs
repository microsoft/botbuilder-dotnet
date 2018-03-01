using Microsoft.Bot.Builder.Middleware;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Integration.NetCore
{
    public interface IBotBuilder
    {
        List<IMiddleware> Middleware { get; }
    }
}
