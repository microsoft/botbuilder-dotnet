using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public interface IWeChatHttpAdapter
    {
        Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot botl, SecretInfo postModal, bool replyAsync, CancellationToken cancellationToken = default);
    }
}
