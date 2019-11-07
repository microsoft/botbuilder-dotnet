// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Skills.Adapters;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Skills
{
    internal delegate Task<object> RouteAction(BotFrameworkSkillHostAdapter skillAdapter, IBot bot, ClaimsIdentity claimsIdentity, HttpRequest httpRequest, GroupCollection parameters, CancellationToken cancellationToken);

    public class BotFrameworkHttpSkillsServer : BotFrameworkSkillHostAdapter
    {
        private static readonly ChannelRoute[] _routes =
        {
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetActivityMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>.*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    return await skillAdapter.GetActivityMembersAsync(bot, claimsIdentity, parameters["conversationId"].Value, parameters["activityId"].Value, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.SendConversationHistory,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/history", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var history = HttpHelper.ReadRequest<Transcript>(httpRequest);
                    return await skillAdapter.SendConversationHistoryAsync(bot, claimsIdentity, parameters["conversationId"].Value, history, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.ReplyToActivity,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var replyToActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                    return await skillAdapter.ReplyToActivityAsync(bot, claimsIdentity, replyToActivity.Conversation.Id, parameters["activityId"].Value, replyToActivity, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.UpdateActivity,
                Pattern = new Regex(@"/PUT:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var updateActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                    return await skillAdapter.UpdateActivityAsync(bot, claimsIdentity, parameters["conversationId"].Value, parameters["activityId"].Value, updateActivity, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.DeleteActivity,
                Pattern = new Regex(@"/DELETE:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    await skillAdapter.DeleteActivityAsync(bot, claimsIdentity, parameters["conversationId"].Value, parameters["activityId"].Value, cancellationToken).ConfigureAwait(false);
                    return null;
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.SendToConversation,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var sendToConversationActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                    return await skillAdapter.SendToConversationAsync(bot, claimsIdentity, parameters["conversationId"].Value, sendToConversationActivity, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.DeleteConversationMember,
                Pattern = new Regex(@"/DELETE:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/members/(?<memberId>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    await skillAdapter.DeleteConversationMemberAsync(bot, claimsIdentity, parameters["conversationId"].Value, parameters["memberId"].Value, cancellationToken).ConfigureAwait(false);
                    return null;
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.UploadAttachment,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/attachments", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var uploadAttachment = HttpHelper.ReadRequest<AttachmentData>(httpRequest);
                    return await skillAdapter.UploadAttachmentAsync(bot, claimsIdentity, parameters["conversationId"].Value, uploadAttachment, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversationMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    return await skillAdapter.GetConversationMembersAsync(bot, claimsIdentity, parameters["conversationId"].Value, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversationPagedMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/pagedmember", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var pageSize = string.IsNullOrWhiteSpace(parameters["pageSize"].Value) ? -1 : int.Parse(parameters["pageSize"].Value, CultureInfo.InvariantCulture);
                    return await skillAdapter.GetConversationPagedMembersAsync(bot, claimsIdentity, parameters["conversationId"].Value, pageSize, parameters["continuationToken"].Value, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversations,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    return await skillAdapter.GetConversationsAsync(bot, claimsIdentity, parameters["conversationId"].Value, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.CreateConversation,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (skillAdapter, bot, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var conversationParameters = HttpHelper.ReadRequest<ConversationParameters>(httpRequest);
                    return await skillAdapter.CreateConversationAsync(bot, claimsIdentity, parameters["conversationId"].Value, conversationParameters, cancellationToken).ConfigureAwait(false);
                }
            }
        };

        private readonly IChannelProvider _channelProvider;
        private readonly ICredentialProvider _credentialsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpSkillsServer"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="adapter">adapter that this skillAdapter is bound to.</param>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <exception cref="ArgumentNullException">throw ArgumentNullException.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the Use(<see cref="IMiddleware"/>) method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkHttpSkillsServer(
            BotFrameworkHttpAdapter adapter,
            ICredentialProvider credentialProvider,
            IChannelProvider channelProvider = null,
            HttpClient customHttpClient = null,
            ILogger logger = null)
            : base(adapter, credentialProvider, channelProvider, customHttpClient, logger)
        {
            _credentialsProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _channelProvider = channelProvider;
        }

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
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
                var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, _credentialsProvider, _channelProvider, "unknown").ConfigureAwait(false);

                var route = GetRoute(httpRequest);
                if (route == null)
                {
                    httpResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                // Execute the route action
                result = await route.Action.Invoke(this, bot, claimsIdentity, httpRequest, route.Parameters, cancellationToken).ConfigureAwait(false);
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
