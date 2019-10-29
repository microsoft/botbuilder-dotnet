// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Net;
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

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Skills
{
    public class BotFrameworkHttpSkillsServer
    {
        private static readonly ChannelRoute[] _routes =
        {
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetActivityMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>.*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.SendConversationHistory,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/history", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.ReplyToActivity,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.UpdateActivity,
                Pattern = new Regex(@"/PUT:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.DeleteActivity,
                Pattern = new Regex(@"/DELETE:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.SendToConversation,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.DeleteConversationMember,
                Pattern = new Regex(@"/DELETE:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/members/(?<memberId>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.UploadAttachment,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/attachments", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversationMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversationPagedMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/pagedmember", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversations,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.CreateConversation,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            }
        };

        private readonly ConfigurationChannelProvider _channelProvider;
        private readonly ConfigurationCredentialProvider _credentialsProvider;

        private readonly BotFrameworkSkillHostAdapter _skillAdapter;

        public BotFrameworkHttpSkillsServer(BotFrameworkSkillHostAdapter skillAdapter, IConfiguration configuration)
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

                switch (route.Method)
                {
                    // [Route("/v3/conversations")]
                    case ChannelApiMethods.CreateConversation:
                        var parameters = HttpHelper.ReadRequest<ConversationParameters>(httpRequest);
                        result = await _skillAdapter.CreateConversationAsync(bot, claimsIdentity, route.Parameters["conversationId"].Value, parameters, cancellationToken).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
                    case ChannelApiMethods.DeleteActivity:
                        // TODO: ask Tom why we use the value from the route and not from the activity here.
                        await _skillAdapter.DeleteActivityAsync(bot, claimsIdentity, route.Parameters["conversationId"].Value, route.Parameters["activityId"].Value, cancellationToken).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/members/{memberId}")]
                    case ChannelApiMethods.DeleteConversationMember:
                        await _skillAdapter.DeleteConversationMemberAsync(bot, claimsIdentity, route.Parameters["conversationId"].Value, route.Parameters["memberId"].Value, cancellationToken).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}/members")]
                    case ChannelApiMethods.GetActivityMembers:
                        result = await _skillAdapter.GetActivityMembersAsync(bot, claimsIdentity, route.Parameters["conversationId"].Value, route.Parameters["activityId"].Value, cancellationToken).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/members")]
                    case ChannelApiMethods.GetConversationMembers:
                        result = await _skillAdapter.GetConversationMembersAsync(bot, claimsIdentity, route.Parameters["conversationId"].Value, cancellationToken).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/pagedmembers")]
                    case ChannelApiMethods.GetConversationPagedMembers:
                        var pageSize = string.IsNullOrWhiteSpace(route.Parameters["pageSize"].Value) ? -1 : int.Parse(route.Parameters["pageSize"].Value, CultureInfo.InvariantCulture);
                        result = await _skillAdapter.GetConversationPagedMembersAsync(bot, claimsIdentity, route.Parameters["conversationId"].Value, pageSize, route.Parameters["continuationToken"].Value, cancellationToken).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations")]
                    case ChannelApiMethods.GetConversations:
                        result = await _skillAdapter.GetConversationsAsync(bot, claimsIdentity, route.Parameters["conversationId"].Value, cancellationToken: cancellationToken).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
                    case ChannelApiMethods.ReplyToActivity:
                        var replyToActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                        result = await _skillAdapter.ReplyToActivityAsync(bot, claimsIdentity, replyToActivity.Conversation.Id, route.Parameters["activityId"].Value, replyToActivity, cancellationToken).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/history")]
                    case ChannelApiMethods.SendConversationHistory:
                        var history = HttpHelper.ReadRequest<Transcript>(httpRequest);
                        result = await _skillAdapter.SendConversationHistoryAsync(bot, claimsIdentity, route.Parameters["conversationId"].Value, history, cancellationToken).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities")]
                    case ChannelApiMethods.SendToConversation:
                        var sendToConversationActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                        result = await _skillAdapter.SendToConversationAsync(bot, claimsIdentity, sendToConversationActivity.Conversation.Id, sendToConversationActivity, cancellationToken).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
                    case ChannelApiMethods.UpdateActivity:
                        var updateActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                        result = await _skillAdapter.UpdateActivityAsync(bot, claimsIdentity, route.Parameters["conversationId"].Value, route.Parameters["activityId"].Value, updateActivity, cancellationToken).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/attachments")]
                    case ChannelApiMethods.UploadAttachment:
                        var uploadAttachment = HttpHelper.ReadRequest<AttachmentData>(httpRequest);
                        result = await _skillAdapter.UploadAttachmentAsync(bot, claimsIdentity, route.Parameters["conversationId"].Value, uploadAttachment, cancellationToken).ConfigureAwait(false);
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
                        Parameters = match.Groups
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
