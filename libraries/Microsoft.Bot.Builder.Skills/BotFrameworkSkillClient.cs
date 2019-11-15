// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// A skill host adapter implements API to forward activity to a skill and 
    /// implements routing ChannelAPI calls from the Skill up through the bot/adapter.
    /// </summary>
    public class BotFrameworkSkillClient
    {
        public const string InvokeActivityName = "SkillEvents.ChannelApiInvoke";
        internal const string BotIdentityKey = "BotIdentity";

        private static readonly HttpClient _defaultHttpClient = new HttpClient();

        // Cache for appCredentials to speed up token acquisition (a token is not requested unless is expired)
        // AppCredentials are cached using appId + skillId (this last parameter is only used if the app credentials are used to call a skill)
        private readonly ConcurrentDictionary<string, AppCredentials> _appCredentialMap = new ConcurrentDictionary<string, AppCredentials>();
        private readonly IChannelProvider _channelProvider;
        private readonly ICredentialProvider _credentialProvider;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly AuthenticationConfiguration _authConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkSkillClient"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <exception cref="ArgumentNullException">throw ArgumentNullException.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the Use(<see cref="IMiddleware"/>) method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkSkillClient(
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfig,
            IChannelProvider channelProvider = null,
            HttpClient customHttpClient = null,
            ILogger logger = null)
        {
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _authConfiguration = authConfig ?? throw new ArgumentNullException(nameof(authConfig));
            _channelProvider = channelProvider;
            _httpClient = customHttpClient ?? _defaultHttpClient;
            _logger = logger ?? NullLogger.Instance;

            // DefaultRequestHeaders are not thread safe so set them up here because this adapter should be a singleton.
            ConnectorClient.AddDefaultRequestHeaders(_httpClient);
        }
        
        /// <summary>
        /// Forwards an activity to a skill (bot).
        /// </summary>
        /// <remarks>NOTE: Forwarding an activity to a skill will flush UserState and ConversationState changes so that skill has accurate state.</remarks>
        /// <param name="botId">The MicrosoftAppId of the bot forwarding the activity.</param>
        /// <param name="skill">A <see cref="BotFrameworkSkill"/> instance with the skill information.</param>
        /// <param name="skillHostEndpoint">The callback Url for the skill host.</param>
        /// <param name="activity">activity to forward.</param>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Async task with optional invokeResponse.</returns>
        public async Task<InvokeResponse> ForwardActivityAsync(string botId, BotFrameworkSkill skill, Uri skillHostEndpoint, Activity activity, CancellationToken cancellationToken)
        {
            var appCredentials = await GetAppCredentialsAsync(botId, skill.AppId).ConfigureAwait(false);
            if (appCredentials == null)
            {
                throw new InvalidOperationException("Unable to get appCredentials to connect to the skill");
            }

            // Get token for the skill call
            var token = await appCredentials.GetTokenAsync().ConfigureAwait(false);

            var activityClone = JObject.FromObject(activity).ToObject<Activity>();

            // TODO use SkillConversation class here instead of hard coded encoding...
            // Encode original bot service URL and ConversationId in the new conversation ID so we can unpack it later.
            // var skillConversation = new SkillConversation() { ServiceUrl = activity.ServiceUrl, ConversationId = activity.Conversation.Id };
            // activity.Conversation.Id = skillConversation.GetSkillConversationId()
            activityClone.Conversation.Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new[]
            {
                activityClone.Conversation.Id,
                activityClone.ServiceUrl
            })));
            activityClone.ServiceUrl = skillHostEndpoint.ToString();
            activityClone.Recipient.Properties["skillId"] = skill.Id;
            using (var jsonContent = new StringContent(JsonConvert.SerializeObject(activityClone, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json"))
            {
                using (var httpRequestMessage = new HttpRequestMessage())
                {
                    httpRequestMessage.Method = HttpMethod.Post;
                    httpRequestMessage.RequestUri = skill.SkillEndpoint;
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    httpRequestMessage.Content = jsonContent;
                    var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (content.Length > 0)
                    {
                        return new InvokeResponse
                        {
                            Status = (int)response.StatusCode,
                            Body = JsonConvert.DeserializeObject(content)
                        };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// GetConversationsAsync() API for Skill.
        /// </summary>
        /// <remarks>
        /// List the Conversations in which this bot has participated.
        /// 
        /// GET from this method with a skip token
        /// 
        /// The return value is a ConversationsResult, which contains an array of
        /// ConversationMembers and a skip token.  If the skip token is not empty, then
        /// there are further values to be returned. Call this method again with the
        /// returned token to get more values.
        /// 
        /// Each ConversationMembers object contains the ID of the conversation and an
        /// array of ChannelAccounts that describe the members of the conversation.
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>conversationId.</param> 
        /// <param name='continuationToken'>skip or continuation token.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for ConversationsResult.</returns>
        internal virtual Task<ConversationsResult> GetConversationsAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, string continuationToken = default, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync<ConversationsResult>(adapter, bot, claimsIdentity, ChannelApiMethods.GetConversations, conversationId, continuationToken);
        }

        /// <summary>
        /// CreateConversation() API for Skill.
        /// </summary>
        /// <remarks>
        /// Create a new Conversation.
        ///
        /// POST to this method with a
        /// * Bot being the bot creating the conversation
        /// * IsGroup set to true if this is not a direct message (default is false)
        /// * Array containing the members to include in the conversation
        ///
        /// The return value is a ResourceResponse which contains a conversation id
        /// which is suitable for use
        /// in the message payload and REST API uris.
        ///
        /// Most channels only support the semantics of bots initiating a direct
        /// message conversation.  An example of how to do that would be:
        ///
        /// var resource = await connector.conversations.CreateConversation(new
        /// ConversationParameters(){ Bot = bot, members = new ChannelAccount[] { new
        /// ChannelAccount("user1") } );
        /// await connect.Conversations.SendToConversationAsync(resource.Id, new
        /// Activity() ... ) ;
        ///
        /// end. 
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>conversationId.</param> 
        /// <param name='parameters'>Parameters to create the conversation from.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a conversation resource response.</returns>
        internal virtual Task<ConversationResourceResponse> CreateConversationAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, ConversationParameters parameters, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync<ConversationResourceResponse>(adapter, bot, claimsIdentity, ChannelApiMethods.CreateConversation, conversationId, parameters);
        }

        /// <summary>
        /// SendToConversation() API for Skill.
        /// </summary>
        /// <remarks>
        /// This method allows you to send an activity to the end of a conversation.
        ///
        /// This is slightly different from ReplyToActivity().
        /// * SendToConversation(conversationId) - will append the activity to the end
        /// of the conversation according to the timestamp or semantics of the channel.
        /// * ReplyToActivity(conversationId,ActivityId) - adds the activity as a reply
        /// to another activity, if the channel supports it. If the channel does not
        /// support nested replies, ReplyToActivity falls back to SendToConversation.
        ///
        /// Use ReplyToActivity when replying to a specific activity in the
        /// conversation.
        ///
        /// Use SendToConversation in all other cases.
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>conversationId.</param> 
        /// <param name='activity'>Activity to send.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        internal virtual Task<ResourceResponse> SendToConversationAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync<ResourceResponse>(adapter, bot, claimsIdentity, ChannelApiMethods.SendToConversation, conversationId, activity);
        }

        /// <summary>
        /// SendConversationHistory() API for Skill.
        /// </summary>
        /// <remarks>
        /// This method allows you to upload the historic activities to the
        /// conversation.
        ///
        /// Sender must ensure that the historic activities have unique ids and
        /// appropriate timestamps. The ids are used by the client to deal with
        /// duplicate activities and the timestamps are used by the client to render
        /// the activities in the right order.
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='transcript'>Transcript of activities.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        internal virtual Task<ResourceResponse> SendConversationHistoryAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, Transcript transcript, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync<ResourceResponse>(adapter, bot, claimsIdentity, ChannelApiMethods.SendConversationHistory, conversationId, transcript);
        }

        /// <summary>
        /// UpdateActivity() API for Skill.
        /// </summary>
        /// <remarks>
        /// Edit an existing activity.
        ///
        /// Some channels allow you to edit an existing activity to reflect the new
        /// state of a bot conversation.
        ///
        /// For example, you can remove buttons after someone has clicked "Approve"
        /// button.
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='activityId'>activityId to update.</param>
        /// <param name='activity'>replacement Activity.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        internal virtual Task<ResourceResponse> UpdateActivityAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync<ResourceResponse>(adapter, bot, claimsIdentity, ChannelApiMethods.UpdateActivity, conversationId, activityId, activity);
        }

        /// <summary>
        /// ReplyToActivity() API for Skill.
        /// </summary>
        /// <remarks>
        /// This method allows you to reply to an activity.
        ///
        /// This is slightly different from SendToConversation().
        /// * SendToConversation(conversationId) - will append the activity to the end
        /// of the conversation according to the timestamp or semantics of the channel.
        /// * ReplyToActivity(conversationId,ActivityId) - adds the activity as a reply
        /// to another activity, if the channel supports it. If the channel does not
        /// support nested replies, ReplyToActivity falls back to SendToConversation.
        ///
        /// Use ReplyToActivity when replying to a specific activity in the
        /// conversation.
        ///
        /// Use SendToConversation in all other cases.
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='activityId'>activityId the reply is to (OPTIONAL).</param>
        /// <param name='activity'>Activity to send.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        internal virtual Task<ResourceResponse> ReplyToActivityAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync<ResourceResponse>(adapter, bot, claimsIdentity, ChannelApiMethods.ReplyToActivity, conversationId, activityId, activity);
        }

        /// <summary>
        /// DeleteActivity() API for Skill.
        /// </summary>
        /// <remarks>
        /// Delete an existing activity.
        ///
        /// Some channels allow you to delete an existing activity, and if successful
        /// this method will remove the specified activity.
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='activityId'>activityId to delete.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        internal virtual Task DeleteActivityAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync(adapter, bot, claimsIdentity, ChannelApiMethods.DeleteActivity, conversationId, activityId);
        }

        /// <summary>
        /// GetConversationMembers() API for Skill.
        /// </summary>
        /// <remarks>
        /// Enumerate the members of a conversation.
        ///
        /// This REST API takes a ConversationId and returns an array of ChannelAccount
        /// objects representing the members of the conversation.
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a response.</returns>
        internal virtual Task<IList<ChannelAccount>> GetConversationMembersAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync<IList<ChannelAccount>>(adapter, bot, claimsIdentity, ChannelApiMethods.GetConversationMembers, conversationId);
        }

        /// <summary>
        /// GetConversationPagedMembers() API for Skill.
        /// </summary>
        /// <remarks>
        /// Enumerate the members of a conversation one page at a time.
        ///
        /// This REST API takes a ConversationId. Optionally a pageSize and/or
        /// continuationToken can be provided. It returns a PagedMembersResult, which
        /// contains an array
        /// of ChannelAccounts representing the members of the conversation and a
        /// continuation token that can be used to get more values.
        ///
        /// One page of ChannelAccounts records are returned with each call. The number
        /// of records in a page may vary between channels and calls. The pageSize
        /// parameter can be used as
        /// a suggestion. If there are no additional results the response will not
        /// contain a continuation token. If there are no members in the conversation
        /// the Members will be empty or not present in the response.
        ///
        /// A response to a request that has a continuation token from a prior request
        /// may rarely return members from a previous request.
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='pageSize'>Suggested page size.</param>
        /// <param name='continuationToken'>Continuation Token.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a response.</returns>
        internal virtual Task<PagedMembersResult> GetConversationPagedMembersAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, int? pageSize = default, string continuationToken = default, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync<PagedMembersResult>(adapter, bot, claimsIdentity, ChannelApiMethods.GetConversationPagedMembers, conversationId, pageSize, continuationToken);
        }

        /// <summary>
        /// DeleteConversationMember() API for Skill.
        /// </summary>
        /// <remarks>
        /// Deletes a member from a conversation.
        ///
        /// This REST API takes a ConversationId and a memberId (of type string) and
        /// removes that member from the conversation. If that member was the last
        /// member
        /// of the conversation, the conversation will also be deleted.
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='memberId'>ID of the member to delete from this conversation.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task.</returns>
        internal virtual Task DeleteConversationMemberAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, string memberId, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync(adapter, bot, claimsIdentity, ChannelApiMethods.DeleteConversationMember, conversationId, memberId);
        }

        /// <summary>
        /// GetActivityMembers() API for Skill.
        /// </summary>
        /// <remarks>
        /// Enumerate the members of an activity.
        ///
        /// This REST API takes a ConversationId and a ActivityId, returning an array
        /// of ChannelAccount objects representing the members of the particular
        /// activity in the conversation.
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='activityId'>Activity ID.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task with result.</returns>
        internal virtual Task<IList<ChannelAccount>> GetActivityMembersAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync<IList<ChannelAccount>>(adapter, bot, claimsIdentity, ChannelApiMethods.GetActivityMembers, conversationId, activityId);
        }

        /// <summary>
        /// UploadAttachment() API for Skill.
        /// </summary>
        /// <remarks>
        /// Upload an attachment directly into a channel's blob storage.
        /// 
        /// This is useful because it allows you to store data in a compliant store
        /// when dealing with enterprises.
        /// 
        /// The response is a ResourceResponse which contains an AttachmentId which is
        /// suitable for using with the attachments API.
        /// </remarks>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='attachmentUpload'>Attachment data.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task with result.</returns>
        internal virtual Task<ResourceResponse> UploadAttachmentAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string conversationId, AttachmentData attachmentUpload, CancellationToken cancellationToken = default)
        {
            return InvokeChannelApiAsync<ResourceResponse>(adapter, bot, claimsIdentity, ChannelApiMethods.UploadAttachment, conversationId, attachmentUpload);
        }

        private async Task InvokeChannelApiAsync(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string method, string conversationId, params object[] args)
        {
            await InvokeChannelApiAsync<object>(adapter, bot, claimsIdentity, method, conversationId, args).ConfigureAwait(false);
        }

        private async Task<T> InvokeChannelApiAsync<T>(BotAdapter adapter, IBot bot, ClaimsIdentity claimsIdentity, string method, string conversationId, params object[] args)
        {
            // make sure there is a channel api middleware
            if (!adapter.MiddlewareSet.Any(mw => mw is ChannelApiMiddleware))
            {
                adapter.MiddlewareSet.Use(new ChannelApiMiddleware(this));
            }

            _logger.LogInformation($"InvokeChannelApiAsync(). Invoking method \"{method}\"");

            var skillConversation = new SkillConversation(conversationId);

            var channelApiInvokeActivity = Activity.CreateInvokeActivity();
            channelApiInvokeActivity.Name = InvokeActivityName;
            channelApiInvokeActivity.ChannelId = "unknown";
            channelApiInvokeActivity.ServiceUrl = skillConversation.ServiceUrl;
            channelApiInvokeActivity.Conversation = new ConversationAccount(id: skillConversation.ConversationId);
            channelApiInvokeActivity.From = new ChannelAccount(id: "unknown");
            channelApiInvokeActivity.Recipient = new ChannelAccount(id: "unknown", role: RoleTypes.Bot);

            var activityPayload = args?.Where(arg => arg is Activity).Cast<Activity>().FirstOrDefault();
            if (activityPayload != null)
            {
                // fix up activityPayload with original conversation.Id and id
                activityPayload.Conversation.Id = skillConversation.ConversationId;
                activityPayload.ServiceUrl = skillConversation.ServiceUrl;

                // use the activityPayload for channel accounts, it will be in From=Bot/Skill Recipient=User, 
                // We want to send it to the bot as From=User, Recipient=Bot so we have correct state context.
                channelApiInvokeActivity.ChannelId = activityPayload.ChannelId;
                channelApiInvokeActivity.From = activityPayload.Recipient;
                channelApiInvokeActivity.Recipient = activityPayload.From;

                // We want ActivityPayload to also be in User->Bot context, if it is outbound it will go through context.SendActivity which will flip outgoing to Bot->User
                // regardless this gives us same memory context of User->Bot which is useful for things like EndOfConversation processing being in the correct memory context.
                activityPayload.From = channelApiInvokeActivity.From;
                activityPayload.Recipient = channelApiInvokeActivity.Recipient;
            }

            var channelApiArgs = new ChannelApiArgs
            {
                Method = method,
                Args = args
            };
            channelApiInvokeActivity.Value = channelApiArgs;

            // send up to the bot to process it...
            await adapter.ProcessActivityAsync(claimsIdentity, (Activity)channelApiInvokeActivity, bot.OnTurnAsync, CancellationToken.None).ConfigureAwait(false);

            if (channelApiArgs.Exception != null)
            {
                throw channelApiArgs.Exception;
            }

            // Return the result that was captured in the middleware handler. 
            return (T)channelApiArgs.Result;
        }
        
        /// <summary>
        /// Gets the application credentials. App Credentials are cached so as to ensure we are not refreshing
        /// token every time.
        /// </summary>
        /// <param name="appId">The application identifier (AAD Id for the bot).</param>
        /// <param name="oAuthScope">The scope for the token, skills will use the Skill App Id. </param>
        /// <returns>App credentials.</returns>
        private async Task<AppCredentials> GetAppCredentialsAsync(string appId, string oAuthScope = null)
        {
            if (appId == null)
            {
                return MicrosoftAppCredentials.Empty;
            }

            var cacheKey = $"{appId}{oAuthScope}";
            if (_appCredentialMap.TryGetValue(cacheKey, out var appCredentials))
            {
                return appCredentials;
            }

            // NOTE: we can't do async operations inside of a AddOrUpdate, so we split access pattern
            var appPassword = await _credentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);
            appCredentials = _channelProvider != null && _channelProvider.IsGovernment() ? new MicrosoftGovernmentAppCredentials(appId, appPassword, _httpClient, _logger) : new MicrosoftAppCredentials(appId, appPassword, _httpClient, _logger, oAuthScope);

            // Cache the credentials for later use
            _appCredentialMap[cacheKey] = appCredentials;
            return appCredentials;
        }
    }
}
