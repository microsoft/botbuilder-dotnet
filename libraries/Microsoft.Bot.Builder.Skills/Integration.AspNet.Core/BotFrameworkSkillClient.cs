// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Skills
{
    internal delegate Task<object> RouteAction(BotAdapter adapter, SkillClient skillClient, IBot bot, ClaimsIdentity claimsIdentity, HttpRequest httpRequest, GroupCollection parameters, CancellationToken cancellationToken);

    public class BotFrameworkSkillClient : SkillClient
    {
        internal const string BotIdentityKey = "BotIdentity";

        private static readonly ChannelRoute[] _routes =
        {
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetActivityMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>.*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    return await skillClient.GetActivityMembersAsync(adapter, bot, claimsIdentity, parameters["conversationId"].Value, parameters["activityId"].Value, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.SendConversationHistory,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/history", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var history = HttpHelper.ReadRequest<Transcript>(httpRequest);
                    return await skillClient.SendConversationHistoryAsync(adapter, bot, claimsIdentity, parameters["conversationId"].Value, history, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.ReplyToActivity,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var replyToActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                    return await skillClient.ReplyToActivityAsync(adapter, bot, claimsIdentity, replyToActivity.Conversation.Id, parameters["activityId"].Value, replyToActivity, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.UpdateActivity,
                Pattern = new Regex(@"/PUT:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var updateActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                    return await skillClient.UpdateActivityAsync(adapter, bot, claimsIdentity, parameters["conversationId"].Value, parameters["activityId"].Value, updateActivity, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.DeleteActivity,
                Pattern = new Regex(@"/DELETE:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    await skillClient.DeleteActivityAsync(adapter, bot, claimsIdentity, parameters["conversationId"].Value, parameters["activityId"].Value, cancellationToken).ConfigureAwait(false);
                    return null;
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.SendToConversation,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var sendToConversationActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                    return await skillClient.SendToConversationAsync(adapter, bot, claimsIdentity, parameters["conversationId"].Value, sendToConversationActivity, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.DeleteConversationMember,
                Pattern = new Regex(@"/DELETE:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/members/(?<memberId>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    await skillClient.DeleteConversationMemberAsync(adapter, bot, claimsIdentity, parameters["conversationId"].Value, parameters["memberId"].Value, cancellationToken).ConfigureAwait(false);
                    return null;
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.UploadAttachment,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/attachments", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var uploadAttachment = HttpHelper.ReadRequest<AttachmentData>(httpRequest);
                    return await skillClient.UploadAttachmentAsync(adapter, bot, claimsIdentity, parameters["conversationId"].Value, uploadAttachment, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversationMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    return await skillClient.GetConversationMembersAsync(adapter, bot, claimsIdentity, parameters["conversationId"].Value, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversationPagedMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/pagedmember", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var pageSize = string.IsNullOrWhiteSpace(parameters["pageSize"].Value) ? -1 : int.Parse(parameters["pageSize"].Value, CultureInfo.InvariantCulture);
                    return await skillClient.GetConversationPagedMembersAsync(adapter, bot, claimsIdentity, parameters["conversationId"].Value, pageSize, parameters["continuationToken"].Value, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversations,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    return await skillClient.GetConversationsAsync(adapter, bot, claimsIdentity, parameters["conversationId"].Value, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.CreateConversation,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (adapter, skillClient, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var conversationParameters = HttpHelper.ReadRequest<ConversationParameters>(httpRequest);
                    return await skillClient.CreateConversationAsync(adapter, bot, claimsIdentity, parameters["conversationId"].Value, conversationParameters, cancellationToken).ConfigureAwait(false);
                }
            }
        };

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
            : base(logger)
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
        /// <param name="turnContext">turnContext.</param>
        /// <param name="skill">A <see cref="BotFrameworkSkill"/> instance with the skill information.</param>
        /// <param name="skillHostEndpoint">The callback Url for the skill host.</param>
        /// <param name="activity">activity to forward.</param>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Async task with optional invokeResponse.</returns>
        public async Task<InvokeResponse> ForwardActivityAsync(ITurnContext turnContext, BotFrameworkSkill skill, Uri skillHostEndpoint, Activity activity, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Received request to forward activity to skill id {skill.Id}.");

            // Pull the current claims identity from TurnState (it is stored there on the way in).
            var identity = (ClaimsIdentity)turnContext.TurnState.Get<IIdentity>(BotIdentityKey);
            if (identity.AuthenticationType.Equals("anonymous", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new NotSupportedException("Anonymous calls are not supported for skills, please ensure your bot is configured with a MicrosoftAppId and Password).");
            }

            // Get current Bot ID from the identity audience claim
            var botAppId = identity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim)?.Value;
            if (string.IsNullOrWhiteSpace(botAppId))
            {
                throw new InvalidOperationException("Unable to get the audience from the current request identity");
            }

            return await ForwardActivityAsync(botAppId, skill, skillHostEndpoint, activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Forwards an activity to a skill (bot).
        /// </summary>
        /// <remarks>NOTE: Forwarding an activity to a skill will flush UserState and ConversationState changes so that skill has accurate state.</remarks>
        /// <param name="botAppId">The MicrosoftAppId of the bot forwarding the activity.</param>
        /// <param name="skill">A <see cref="BotFrameworkSkill"/> instance with the skill information.</param>
        /// <param name="skillHostEndpoint">The callback Url for the skill host.</param>
        /// <param name="activity">activity to forward.</param>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Async task with optional invokeResponse.</returns>
        public async Task<InvokeResponse> ForwardActivityAsync(string botAppId, BotFrameworkSkill skill, Uri skillHostEndpoint, Activity activity, CancellationToken cancellationToken)
        {
            var appCredentials = await GetAppCredentialsAsync(botAppId, skill.AppId).ConfigureAwait(false);
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

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, BotAdapter adapter, IBot bot, CancellationToken cancellationToken = default)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            object result = null;
            var statusCode = (int)HttpStatusCode.OK;
            try
            {
                // grab the auth header from the inbound http request
                var authHeader = httpRequest.Headers["Authorization"];
                var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, _credentialProvider, _channelProvider,  "unknown", _authConfiguration).ConfigureAwait(false);

                var route = GetRoute(httpRequest);
                if (route == null)
                {
                    httpResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                // Execute the route action
                result = await route.Action.Invoke(adapter, this, bot, claimsIdentity, httpRequest, route.Parameters, cancellationToken).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException)
            {
                // handle unauthorized here as this layer creates the http response
                statusCode = (int)HttpStatusCode.Unauthorized;
            }

            HttpHelper.WriteResponse(httpResponse, statusCode, result);
        }

        internal static RouteResult GetRoute(HttpRequest httpRequest)
        {
            var path = $"/{httpRequest.Method}:{httpRequest.Path}";
            foreach (var route in _routes)
            {
                var match = route.Pattern.Match(path);
                if (match.Success)
                {
                    var result = new RouteResult
                    {
                        Method = route.Method,
                        Parameters = match.Groups,
                        Action = route.Action
                    };

                    return result;
                }
            }

            return null;
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

        internal class ChannelRoute
        {
            public Regex Pattern { get; set; }

            public string Method { get; set; }

            public RouteAction Action { get; set; }
        }

        internal class RouteResult
        {
            public string Method { get; set; }

            public GroupCollection Parameters { get; set; }

            public RouteAction Action { get; set; }
        }
    }
}
