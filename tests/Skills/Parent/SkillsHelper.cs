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
        private readonly SkillConversationIdFactoryBase _conversationIdFactory;
        private readonly string _botId;
        private readonly SkillHttpClient _skillHttpClient;
        private readonly BotAdapter _botAdapter;
        private readonly IExtendedUserTokenProvider _tokenExchangeProvider;
        private readonly string _parentConnectionName;
        private readonly Uri _callbackUri;

        public SkillsHelper(IConfiguration configuration, SkillHttpClient skillHttpClient, BotAdapter botAdapter, SkillConversationIdFactoryBase conversationIdFactory)
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
            _conversationIdFactory = conversationIdFactory;
        }

        public async Task<InvokeResponse<ExpectedReplies>> PostActivityAsync(Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _skillHttpClient.PostActivityAsync<ExpectedReplies>(_botId, _botFrameworkSkill, _callbackUri, activity, cancellationToken);
            return response;
        }

        public async Task<bool> InterceptOAuthCards(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var activity = activities.FirstOrDefault(a => a.Type == ActivityTypes.Message);
            if (activity != null)
            {
                return await InterceptOAuthCards(turnContext, activity, cancellationToken);
            }

            return false;
        }

        public async Task<bool> InterceptOAuthCards(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            if (activity.Attachments != null)
            {
                foreach (var attachment in activity.Attachments.Where(a => a?.ContentType == OAuthCard.ContentType))
                {
                    var oauthCard = ((JObject)attachment.Content).ToObject<OAuthCard>();

                    if (oauthCard.TokenExchangeResource != null)
                    {
                        try
                        {
                            // AAD token exchange
                            var result = await _tokenExchangeProvider.ExchangeTokenAsync(
                                turnContext,
                                _parentConnectionName,
                                activity.Recipient.Id,
                                new TokenExchangeRequest() { Uri = oauthCard.TokenExchangeResource.Uri }).ConfigureAwait(false);

                            if (!string.IsNullOrEmpty(result.Token))
                            {
                                // Send an Invoke back to the Skill
                                return await SendTokenExchangeInvokeToSkill(turnContext, activity, result.Token, oauthCard.ConnectionName, default(CancellationToken)).ConfigureAwait(false);
                            }
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        private async Task<bool> SendTokenExchangeInvokeToSkill(ITurnContext turnContext, Activity incomingActivity, string token, string connectionName, CancellationToken cancellationToken)
        {
            var activity = incomingActivity.CreateReply() as Activity;
            activity.Type = ActivityTypes.Invoke;
            activity.Name = SignInConstants.TokenExchangeOperationName;
            activity.Value = new TokenExchangeInvokeRequest()
            {
                Token = token,
                ConnectionName = connectionName,
            };

            var skillConversationReference = await _conversationIdFactory.GetSkillConversationReferenceAsync(incomingActivity.Conversation.Id, cancellationToken).ConfigureAwait(false);
            activity.Conversation = skillConversationReference.ConversationReference.Conversation;
            activity.ServiceUrl = skillConversationReference.ConversationReference.ServiceUrl;

            // route the activity to the skill
            var response = await PostActivityAsync(activity, cancellationToken);

            // Check response status: true if success, false if failure
            var success = IsSucessStatusCode(response.Status);
            if (success)
            {
                await turnContext.SendActivityAsync("token exchange successful");
            }
            else
            {
                await turnContext.SendActivityAsync("token exchange failed");
            }

            return success;
        }

        private bool IsSucessStatusCode(int statusCode)
        {
            return statusCode >= 200 && statusCode <= 299;
        }
    }
}
