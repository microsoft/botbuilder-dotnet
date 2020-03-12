using System;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class TokenExchangeSkillHandler : SkillHandler
    {
        private readonly BotAdapter _adapter;
        private readonly IExtendedUserTokenProvider _tokenExchangeProvider;
        private readonly string _botId;
        private readonly SkillConversationIdFactoryBase _conversationIdFactory;
        private readonly SkillsHelper _skillsHelper;
        private readonly string _parentConnectionName;

        public TokenExchangeSkillHandler(
            BotAdapter adapter,
            IBot bot,
            IConfiguration configuration,
            SkillConversationIdFactoryBase conversationIdFactory,
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfig,
            SkillsHelper skillsHelper,
            IChannelProvider channelProvider = null,
            ILogger logger = null)
            : base(adapter, bot, conversationIdFactory, credentialProvider, authConfig, channelProvider, logger)
        {
            _adapter = adapter;
            _tokenExchangeProvider = adapter as IExtendedUserTokenProvider;
            _conversationIdFactory = conversationIdFactory;
            _skillsHelper = skillsHelper;

            _botId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
        }

        protected override async Task<ResourceResponse> OnSendToConversationAsync(ClaimsIdentity claimsIdentity, string conversationId, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var turnContext = new TurnContext(_adapter, activity))
            {
                turnContext.TurnState.Add<IIdentity>("BotIdentity", claimsIdentity);

                if (await _skillsHelper.InterceptOAuthCards(turnContext, activity, cancellationToken))
                {
                    return new ResourceResponse(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                }
            }

            return await base.OnSendToConversationAsync(claimsIdentity, conversationId, activity, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<ResourceResponse> OnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var turnContext = new TurnContext(_adapter, activity))
            {
                turnContext.TurnState.Add<IIdentity>("BotIdentity", claimsIdentity);

                if (await _skillsHelper.InterceptOAuthCards(turnContext, activity, cancellationToken))
                {
                    return new ResourceResponse(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                }
            }

            return await base.OnReplyToActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
        }
    }
}
