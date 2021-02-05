using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Contoso.CustomConfigAdapter
{
    public class ContosoAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        public const string Kind = "Contoso.ContosoAdapter";

        private readonly ContosoAdapterOptions _options;

        public ContosoAdapter(IOptions<ContosoAdapterOptions> options)
        {
            this._options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public ContosoAdapter(ContosoAdapterOptions options)
        {
            this._options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException(_options?.ContosoSkillId);
        }

        public Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
