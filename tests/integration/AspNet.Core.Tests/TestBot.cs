using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    internal sealed class TestBot : IBot
    {
        public Task OnTurn(ITurnContext turnContext)
        {
            throw new NotImplementedException();
        }
    }
}
