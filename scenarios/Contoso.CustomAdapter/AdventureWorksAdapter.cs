using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Contoso.CustomAdapter
{
    public class AdventureWorksAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        [JsonProperty("$kind")]
        public const string Kind = "Contoso.AdventureWorksAdapter";

        public AdventureWorksAdapter(IConfiguration configuration)
        {
        }

        public StringExpression AdventureWorksSkillId { get; set; }

        public StringExpression AdventureWorksSecretKey { get; set; }

        public bool AllowSuggestedActions { get; set; } = true;

        public string ApiPath { get; set; } = "adventureworks";

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
