// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public class BotFrameworkChannelHttpServer
    {
        private static readonly ChannelRoute[] _routes =
        {
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.GetActivityMembers,
                Pattern = new Regex(@"/GET:/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>.*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.ReplyToActivity,
                Pattern = new Regex(@"/POST:/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.UpdateActivity,
                Pattern = new Regex(@"/PUT:/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.DeleteActivity,
                Pattern = new Regex(@"/DELETE:/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.SendToConversation,
                Pattern = new Regex(@"/POST:/v3/conversations/(?<conversationId>[^\s/]*)/activities", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.SendConversationHistory,
                Pattern = new Regex(@"/POST:/v3/conversations/(?<conversationId>[^\s/]*)/activities/history", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.DeleteConversationMember,
                Pattern = new Regex(@"/DELETE:/v3/conversations/(?<conversationId>[^\s/]*)/members/(?<memberId>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.UploadAttachment,
                Pattern = new Regex(@"/POST:/v3/conversations/(?<conversationId>[^\s/]*)/attachments", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.GetConversationMembers,
                Pattern = new Regex(@"/GET:/v3/conversations/(?<conversationId>[^\s/]*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.GetConversationPagedMembers,
                Pattern = new Regex(@"/GET:/v3/conversations/(?<conversationId>[^\s/]*)/pagedmember", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.GetConversations,
                Pattern = new Regex(@"/GET:/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
            new ChannelRoute
            {
                Method = BotFrameworkChannelServerMethods.CreateConversation,
                Pattern = new Regex(@"/POST:/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },
        };

        private readonly BotFrameworkHttpAdapter _adapter;
        private readonly string _botAppId;

        public BotFrameworkChannelHttpServer(BotFrameworkHttpAdapter adapter, IConfiguration configuration)
        {
            _adapter = adapter;
            _botAppId = configuration.GetValue<string>("MicrosoftAppId");
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

            try
            {
                // grab the auth header from the inbound http request
                var authHeader = httpRequest.Headers["Authorization"];
                
                // TODO: authenticate header here
                //var claimsIdentity = await JwtTokenValidation.AuthenticateRequest(activity, authHeader, _credentialProvider, _channelProvider, _authConfiguration, _httpClient).ConfigureAwait(false);
                switch (route.Method)
                {
                    case BotFrameworkChannelServerMethods.CreateConversation:
                        // TODO: how do we handle this one?
                        throw new NotImplementedException("CreateConversation is not supported for skills");

                    case BotFrameworkChannelServerMethods.DeleteActivity:
                        await DeleteActivityAsync(httpResponse, route, cancellationToken).ConfigureAwait(false);
                        break;

                    case BotFrameworkChannelServerMethods.DeleteConversationMember:
                        await DeleteConversationMemberAsync(httpResponse, route, cancellationToken).ConfigureAwait(false);
                        break;

                    case BotFrameworkChannelServerMethods.GetActivityMembers:
                        await GetActivityMembersAsync(httpResponse, route, cancellationToken).ConfigureAwait(false);
                        break;

                    case BotFrameworkChannelServerMethods.GetConversationMembers:
                        await GetConversationMembersAsync(httpResponse, route, cancellationToken).ConfigureAwait(false);
                        break;

                    case BotFrameworkChannelServerMethods.GetConversationPagedMembers:
                        await GetConversationPagedMembersAsync(httpResponse, route, cancellationToken).ConfigureAwait(false);
                        break;

                    case BotFrameworkChannelServerMethods.GetConversations:
                        // TODO: how do we handle this one?
                        throw new NotImplementedException("GetConversations is not supported for skills");

                    case BotFrameworkChannelServerMethods.ReplyToActivity:
                        await ReplyToActivityAsync(httpRequest, httpResponse, bot, route, cancellationToken).ConfigureAwait(false);
                        break;

                    case BotFrameworkChannelServerMethods.SendConversationHistory:
                        // TODO: how do we handle this one?
                        throw new NotImplementedException("SendConversationHistory is not supported for skills");

                    case BotFrameworkChannelServerMethods.SendToConversation:
                        // TODO: how do we handle this one?
                        throw new NotImplementedException("SendToConversation is not supported for skills");

                    case BotFrameworkChannelServerMethods.UpdateActivity:
                        await UpdateActivityAsync(httpRequest, httpResponse, bot, route, cancellationToken).ConfigureAwait(false);
                        break;

                    case BotFrameworkChannelServerMethods.UploadAttachment:
                        await UploadAttachmentAsync(httpRequest, httpResponse, bot, route, cancellationToken).ConfigureAwait(false);
                        break;

                    default:
                        httpResponse.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // handle unauthorized here as this layer creates the http response
                httpResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }

        internal async Task DeleteActivityAsync(HttpResponse httpResponse, RouteResult route, CancellationToken cancellationToken)
        {
            var conversationId = route.Groups["conversationId"].Value;
            var activityId = route.Groups["activityId"].Value;
            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            await _adapter.ContinueConversationAsync(
                _botAppId,
                originalConversationReference,
                async (context, ct) =>
                {
                    await context.DeleteActivityAsync(activityId, ct).ConfigureAwait(false);
                },
                cancellationToken).ConfigureAwait(false);

            HttpHelper.WriteResponse(httpResponse, (int)HttpStatusCode.OK);
        }

        internal async Task DeleteConversationMemberAsync(HttpResponse httpResponse, RouteResult route, CancellationToken cancellationToken)
        {
            var conversationId = route.Groups["conversationId"].Value;
            var memberId = route.Groups["memberId"].Value;
            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            await _adapter.ContinueConversationAsync(
                _botAppId,
                originalConversationReference,
                async (context, ct) =>
                {
                    await context.Adapter.DeleteConversationMemberAsync(context, memberId, ct).ConfigureAwait(false);
                },
                cancellationToken).ConfigureAwait(false);
            HttpHelper.WriteResponse(httpResponse, (int)HttpStatusCode.OK);
        }

        internal async Task GetActivityMembersAsync(HttpResponse httpResponse, RouteResult route, CancellationToken cancellationToken)
        {
            var conversationId = route.Groups["conversationId"].Value;
            var activityId = route.Groups["activityId"].Value;
            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            ChannelAccount[] accounts = null;
            await _adapter.ContinueConversationAsync(
                _botAppId,
                originalConversationReference,
                async (context, ct) =>
                {
                    var result = await context.Adapter.GetActivityMembersAsync(context, activityId, ct).ConfigureAwait(false);
                    accounts = result.ToArray();
                },
                cancellationToken).ConfigureAwait(false);
            HttpHelper.WriteResponse(httpResponse, (int)HttpStatusCode.OK, accounts);
        }

        internal virtual async Task GetConversationMembersAsync(HttpResponse httpResponse, RouteResult route, CancellationToken cancellationToken)
        {
            var conversationId = route.Groups["conversationId"].Value;
            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            ChannelAccount[] accounts = null;
            await _adapter.ContinueConversationAsync(
                _botAppId,
                originalConversationReference,
                async (context, ct) =>
                {
                    var result = await context.Adapter.GetConversationMembersAsync(context, ct).ConfigureAwait(false);
                    accounts = result.ToArray();
                },
                CancellationToken.None).ConfigureAwait(false);
            HttpHelper.WriteResponse(httpResponse, (int)HttpStatusCode.OK, accounts);
        }

        internal virtual async Task GetConversationPagedMembersAsync(HttpResponse httpResponse, RouteResult route, CancellationToken cancellationToken)
        {
            var conversationId = route.Groups["conversationId"].Value;
            var pageSize = string.IsNullOrWhiteSpace(route.Groups["pageSize"].Value) ? -1 : int.Parse(route.Groups["pageSize"].Value, CultureInfo.InvariantCulture);
            var continuationToken = route.Groups["continuationToken"].Value;
            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            PagedMembersResult result = null;

            await _adapter.ContinueConversationAsync(
                _botAppId,
                originalConversationReference,
                async (context, ct) =>
                {
                    result = await context.Adapter.GetConversationPagedMembersAsync(context, pageSize, continuationToken, ct).ConfigureAwait(false);
                },
                cancellationToken).ConfigureAwait(false);
            HttpHelper.WriteResponse(httpResponse, (int)HttpStatusCode.OK, result);
        }

        internal virtual async Task ReplyToActivityAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, RouteResult route, CancellationToken cancellationToken)
        {
            var activity = HttpHelper.ReadRequest(httpRequest);
            var conversationId = route.Groups["conversationId"].Value;
            var activityId = route.Groups["activityId"].Value;

            var conversationInfo = GetConversationInfo(conversationId);
            activity.ServiceUrl = conversationInfo.ServiceUrl;
            activity.Conversation.Id = conversationInfo.ConversationId;

            var originalConversationReference = activity.GetConversationReference();
            originalConversationReference.Bot = activity.From;
            originalConversationReference.User = activity.Recipient;

            ResourceResponse resourceResponse = null;
            if (activity.Type == ActivityTypes.EndOfConversation || activity.Type == ActivityTypes.Event)
            {
                activity.ApplyConversationReference(originalConversationReference, isIncoming: true);

                // TEMPORARY claim
                var claimsIdentity = new ClaimsIdentity(new List<Claim>(), "anonymous");

                // send up to the bot 
                // TODO: WE NEED PROCESSACTIVITY TO BE ON BOTADAPTER.CS
                await _adapter.ProcessActivityAsync(claimsIdentity, activity, bot.OnTurnAsync, CancellationToken.None).ConfigureAwait(false);
                resourceResponse = new ResourceResponse(id: Guid.NewGuid().ToString("N"));
            }
            else
            {
                await _adapter.ContinueConversationAsync(
                    _botAppId,
                    originalConversationReference,
                    async (context, ct) =>
                    {
                        activity.ApplyConversationReference(originalConversationReference);
                        activity.ReplyToId = activityId;
                        resourceResponse = await context.SendActivityAsync(activity, ct).ConfigureAwait(false);
                    },
                    cancellationToken).ConfigureAwait(false);
            }

            HttpHelper.WriteResponse(httpResponse, (int)HttpStatusCode.OK, resourceResponse);
        }

        internal virtual async Task UpdateActivityAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, RouteResult route, CancellationToken cancellationToken)
        {
            var activity = HttpHelper.ReadRequest(httpRequest);
            var conversationId = route.Groups["conversationId"].Value;

            ResourceResponse resourceResponse = null;
            var conversationInfo = GetConversationInfo(conversationId);
            activity.ServiceUrl = conversationInfo.ServiceUrl;
            activity.Conversation.Id = conversationInfo.ConversationId;

            var originalConversationReference = activity.GetConversationReference();
            originalConversationReference.Bot = activity.From;
            originalConversationReference.User = activity.Recipient;

            await _adapter.ContinueConversationAsync(
                _botAppId,
                originalConversationReference,
                async (context, ct) =>
                {
                    resourceResponse = await context.UpdateActivityAsync(activity, ct).ConfigureAwait(false);
                },
                cancellationToken).ConfigureAwait(false);

            HttpHelper.WriteResponse(httpResponse, (int)HttpStatusCode.OK, resourceResponse);
        }

        internal virtual async Task UploadAttachmentAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, RouteResult route, CancellationToken cancellationToken)
        {
            var conversationId = route.Groups["conversationId"].Value;
            var attachmentUpload = HttpHelper.ReadAttachmentData(httpRequest);

            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            ResourceResponse response = null;
            await _adapter.ContinueConversationAsync(
                _botAppId,
                originalConversationReference,
                async (context, ct) =>
                {
                    response = await context.Adapter.UploadAttachment(context, attachmentUpload, ct).ConfigureAwait(false);
                },
                cancellationToken).ConfigureAwait(false);

            HttpHelper.WriteResponse(httpResponse, (int)HttpStatusCode.OK, response);
        }

        private static ConversationInfo GetConversationInfo(string skillConversationId)
        {
            var parts = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Convert.FromBase64String(skillConversationId)));
            return new ConversationInfo()
            {
                ServiceUrl = parts[0],
                ConversationId = parts[1],
            };
        }

        private static ConversationReference GetConversationReferenceFromInfo(ConversationInfo conversationInfo)
        {
            var originalConversationReference = new ConversationReference()
            {
                ChannelId = "Skill" /* skillId from claims */,
                ServiceUrl = conversationInfo.ServiceUrl,
                Conversation = new ConversationAccount(id: conversationInfo.ConversationId),
                Bot = new ChannelAccount(id: "unknown", role: RoleTypes.Bot),
                User = new ChannelAccount(id: "unknown", role: "Skill"),
            };
            return originalConversationReference;
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
                        Groups = match.Groups,
                    };
                    return result;
                }
            }

            return null;
        }

        internal class RouteResult
        {
            public string Method { get; set; }

            public GroupCollection Groups { get; set; }
        }

        internal class ChannelRoute
        {
            public Regex Pattern { get; set; }

            public string Method { get; set; }
        }

        private static class BotFrameworkChannelServerMethods
        {
            public const string CreateConversation = "BotFramework.CreateConversation";
            public const string DeleteActivity = "BotFramework.DeleteActivityAsync";
            public const string DeleteConversationMember = "BotFramework.DeleteConversationMemberAsync";
            public const string GetActivityMembers = "BotFramework.GetActivityMembersAsync";
            public const string GetConversationMembers = "BotFramework.GetConversationMembersAsync";
            public const string GetConversationPagedMembers = "BotFramework.GetConversationPagedMembersAsync";
            public const string GetConversations = "BotFramework.GetConversationsAsync";
            public const string ReplyToActivity = "BotFramework.ReplyToActivityAsync";
            public const string SendConversationHistory = "BotFramework.SendConversationHistory";
            public const string SendToConversation = "BotFramework.SendToConversation";
            public const string UpdateActivity = "BotFramework.UpdateActivityAsync";
            public const string UploadAttachment = "BotFramework.UploadAttachmentAsync";
        }

        private class ConversationInfo
        {
            public string ServiceUrl { get; set; }

            public string ConversationId { get; set; }
        }
    }
}
