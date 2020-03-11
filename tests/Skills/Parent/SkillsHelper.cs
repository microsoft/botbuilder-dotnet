using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{
    public class SkillsHelper
    {
        private readonly BotFrameworkSkill _botFrameworkSkill;
        private readonly string _botId;
        private readonly SkillHttpClient _skillHttpClient;
        private readonly BotAdapter _botAdapter;
        private readonly IExtendedUserTokenProvider _tokenExchangeProvider;
        private readonly string _parentConnectionName;
        private readonly Uri _callbackUri;

        public SkillsHelper(IConfiguration configuration, SkillHttpClient skillHttpClient, BotAdapter botAdapter)
        {
            // We use a single skill in this example.
            var section = configuration.GetSection("BotFrameworkSkill");
            _botFrameworkSkill = section?.Get<BotFrameworkSkill>();
            _botId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            _skillHttpClient = skillHttpClient;
            _botAdapter = botAdapter;
            _tokenExchangeProvider = botAdapter as IExtendedUserTokenProvider;
            _parentConnectionName = configuration.GetSection("ConnectionName")?.Value;
            _callbackUri = new Uri(configuration.GetSection("CallbackUri")?.Value);
        }

        public async Task<InvokeResponse<ExpectedReplies>> PostActivityAsync(Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _skillHttpClient.PostActivityAsync<ExpectedReplies>(_botId, _botFrameworkSkill, _callbackUri, activity, cancellationToken);
            return response;
        }

        public async Task<bool> InterceptOAuthCards(Activity[] activities, CancellationToken cancellationToken)
        {
            var activity = activities.FirstOrDefault();
            if (activity != null)
            {
                return await InterceptOAuthCards(activity, cancellationToken);
            }

            return false;
        }

        public async Task<bool> InterceptOAuthCards(Activity activity, CancellationToken cancellationToken)
        {
            if (activity.Attachments != null)
            {
                foreach (var attachment in activity.Attachments.Where(a => a?.ContentType == OAuthCard.ContentType))
                {
                    var oauthCard = ((JObject)attachment.Content).ToObject<OAuthCard>();

                    if (oauthCard.TokenExchangeResource != null)
                    {
                        using (var context = new TurnContext(_botAdapter, activity))
                        {
                            // AAD token exchange
                            var result = await _tokenExchangeProvider.ExchangeTokenAsync(
                                context,
                                _parentConnectionName,
                                context.Activity.Recipient.Id,
                                new TokenExchangeRequest() { Uri = oauthCard.TokenExchangeResource.Uri }).ConfigureAwait(false);

                            if (!string.IsNullOrEmpty(result.Token))
                            {
                                // Send an Invoke back to the Skill
                                return await SendTokenExchangeInvokeToSkill(activity, result.Token, oauthCard.ConnectionName, default(CancellationToken)).ConfigureAwait(false);
                            }

                            return false;
                        }
                    }
                }
            }

            return false;
        }

        private async Task<bool> SendTokenExchangeInvokeToSkill(Activity incomingActivity, string token, string connectionName, CancellationToken cancellationToken)
        {
            var activity = incomingActivity.CreateReply() as Activity;
            activity.Type = ActivityTypes.Invoke;
            activity.Name = SignInConstants.TokenExchangeOperationName;
            activity.Value = new TokenExchangeInvokeRequest()
            {
                Token = token,
                ConnectionName = connectionName,
            };

            //var conversationReference = await _conversationIdFactory.GetConversationReferenceAsync(incomingActivity.Conversation.Id, cancellationToken).ConfigureAwait(false);
            //activity.Conversation = conversationReference.Conversation;

            // route the activity to the skill
            var response = await PostActivityAsync(activity, cancellationToken);

            // Check response status: true if success, false if failure
            return IsSucessStatusCode(response.Status);
        }

        private bool IsSucessStatusCode(int statusCode)
        {
            return statusCode >= 200 && statusCode <= 299;
        }
    }
}
