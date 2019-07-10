using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResult;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class WeChatAttachmentStorage : IWeChatStorage<WeChatJsonResult>
    {
        public static readonly WeChatAttachmentStorage Instance = new WeChatAttachmentStorage();

        private readonly IStorage storage;

        public WeChatAttachmentStorage()
        {
            this.storage = new MemoryStorage();
        }

        public WeChatAttachmentStorage(IStorage storage)
        {
            this.storage = storage;
        }

        public async Task SaveAsync(string key, WeChatJsonResult value, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dict = new Dictionary<string, object>
            {
                { key, value },
            };
            await this.storage.WriteAsync(dict, cancellationToken);
        }

        public async Task<WeChatJsonResult> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var keys = new string[] { key };
            var result = await this.storage.ReadAsync<WeChatJsonResult>(keys, cancellationToken);
            result.TryGetValue(key, out var wechatResult);
            return wechatResult;
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var keys = new string[] { key };
            await this.storage.DeleteAsync(keys, cancellationToken);
        }
    }
}
