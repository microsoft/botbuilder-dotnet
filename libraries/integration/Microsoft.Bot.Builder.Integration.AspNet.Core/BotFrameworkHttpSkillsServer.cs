// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Skills.Adapters;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public class BotFrameworkHttpSkillsServer
    {
        private static readonly ChannelRoute[] _routes =
        {
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetActivityMembers,
                Pattern = new Regex(@"/GET:/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>.*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.ReplyToActivity,
                Pattern = new Regex(@"/POST:/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.UpdateActivity,
                Pattern = new Regex(@"/PUT:/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.DeleteActivity,
                Pattern = new Regex(@"/DELETE:/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.SendToConversation,
                Pattern = new Regex(@"/POST:/v3/conversations/(?<conversationId>[^\s/]*)/activities", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.SendConversationHistory,
                Pattern = new Regex(@"/POST:/v3/conversations/(?<conversationId>[^\s/]*)/activities/history", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.DeleteConversationMember,
                Pattern = new Regex(@"/DELETE:/v3/conversations/(?<conversationId>[^\s/]*)/members/(?<memberId>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.UploadAttachment,
                Pattern = new Regex(@"/POST:/v3/conversations/(?<conversationId>[^\s/]*)/attachments", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversationMembers,
                Pattern = new Regex(@"/GET:/v3/conversations/(?<conversationId>[^\s/]*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversationPagedMembers,
                Pattern = new Regex(@"/GET:/v3/conversations/(?<conversationId>[^\s/]*)/pagedmember", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversations,
                Pattern = new Regex(@"/GET:/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.CreateConversation,
                Pattern = new Regex(@"/POST:/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
        };

        private readonly BotFrameworkSkillAdapter _skillAdapter;
        private readonly ConfigurationChannelProvider _channelProvider;
        private readonly ConfigurationCredentialProvider _credentialsProvider;

        public BotFrameworkHttpSkillsServer(BotFrameworkSkillAdapter skillAdapter, IConfiguration configuration)
        {
            // adapter to use for calling back to channel
            _skillAdapter = skillAdapter;
            
            // _botAppId = configuration.GetValue<string>("MicrosoftAppId");
            _credentialsProvider = new ConfigurationCredentialProvider(configuration);
            _channelProvider = new ConfigurationChannelProvider(configuration);
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

            var route = GetRoute(httpRequest);

            if (route == null)
            {
                httpResponse.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            object result = null;
            var statusCode = (int)HttpStatusCode.OK;
            try
            {
                // grab the auth header from the inbound http request
                var authHeader = httpRequest.Headers["Authorization"];
                var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, _credentialsProvider, _channelProvider, "unknown").ConfigureAwait(false);
                
                switch (route.Method)
                {
                    // [Route("/v3/conversations")]
                    case ChannelApiMethods.CreateConversation:
                        var parameters = await HttpHelper.ReadRequestAsync<ConversationParameters>(httpRequest, cancellationToken).ConfigureAwait(false);
                        result = await _skillAdapter.CreateConversationAsync(claimsIdentity, route.Parameters["conversationId"].Value, parameters).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
                    case ChannelApiMethods.DeleteActivity:
                        // TODO: ask Tom why we use the value from the route and not from the activity here.
                        await _skillAdapter.DeleteActivityAsync(claimsIdentity, route.Parameters["conversationId"].Value, route.Parameters["activityId"].Value).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/members/{memberId}")]
                    case ChannelApiMethods.DeleteConversationMember:
                        await _skillAdapter.DeleteConversationMemberAsync(claimsIdentity, route.Parameters["conversationId"].Value, route.Parameters["memberId"].Value).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}/members")]
                    case ChannelApiMethods.GetActivityMembers:
                        result = await _skillAdapter.GetActivityMembersAsync(claimsIdentity, route.Parameters["conversationId"].Value, route.Parameters["activityId"].Value).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/members")]
                    case ChannelApiMethods.GetConversationMembers:
                        result = await _skillAdapter.GetConversationMembersAsync(claimsIdentity, route.Parameters["conversationId"].Value).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/pagedmembers")]
                    case ChannelApiMethods.GetConversationPagedMembers:
                        var pageSize = string.IsNullOrWhiteSpace(route.Parameters["pageSize"].Value) ? -1 : int.Parse(route.Parameters["pageSize"].Value, CultureInfo.InvariantCulture);
                        result = await _skillAdapter.GetConversationPagedMembersAsync(claimsIdentity, route.Parameters["conversationId"].Value, pageSize, route.Parameters["continuationToken"].Value).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations")]
                    case ChannelApiMethods.GetConversations:
                        result = await _skillAdapter.GetConversationsAsync(claimsIdentity, route.Parameters["conversationId"].Value).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
                    case ChannelApiMethods.ReplyToActivity:
                        var replyToActivity = await HttpHelper.ReadRequestAsync<Activity>(httpRequest, cancellationToken).ConfigureAwait(false);
                        result = await _skillAdapter.ReplyToActivityAsync(claimsIdentity, replyToActivity.Conversation.Id, route.Parameters["activityId"].Value, replyToActivity).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/history")]
                    case ChannelApiMethods.SendConversationHistory:
                        var history = await HttpHelper.ReadRequestAsync<Transcript>(httpRequest, cancellationToken).ConfigureAwait(false);
                        result = await _skillAdapter.SendConversationHistoryAsync(claimsIdentity, route.Parameters["conversationId"].Value, history).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities")]
                    case ChannelApiMethods.SendToConversation:
                        var sendToConversationActivity = await HttpHelper.ReadRequestAsync<Activity>(httpRequest, cancellationToken).ConfigureAwait(false);
                        result = await _skillAdapter.SendToConversationAsync(claimsIdentity, sendToConversationActivity.Conversation.Id, sendToConversationActivity).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
                    case ChannelApiMethods.UpdateActivity:
                        var updateActivity = await HttpHelper.ReadRequestAsync<Activity>(httpRequest, cancellationToken).ConfigureAwait(false);
                        result = await _skillAdapter.UpdateActivityAsync(claimsIdentity, route.Parameters["conversationId"].Value, route.Parameters["activityId"].Value, updateActivity).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/attachments")]
                    case ChannelApiMethods.UploadAttachment:
                        var uploadAttachment = await HttpHelper.ReadRequestAsync<AttachmentData>(httpRequest, cancellationToken).ConfigureAwait(false);
                        result = await _skillAdapter.UploadAttachmentAsync(claimsIdentity, route.Parameters["conversationId"].Value, uploadAttachment).ConfigureAwait(false);
                        break;

                    default:
                        httpResponse.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // handle unauthorized here as this layer creates the http response
                statusCode = (int)HttpStatusCode.Unauthorized;
            }

            HttpHelper.WriteResponse(httpResponse, statusCode, result);
        }

        private RouteResult GetRoute(HttpRequest httpRequest)
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
        }

        internal class RouteResult
        {
            public string Method { get; set; }

            public GroupCollection Parameters { get; set; }
        }
    }
}
