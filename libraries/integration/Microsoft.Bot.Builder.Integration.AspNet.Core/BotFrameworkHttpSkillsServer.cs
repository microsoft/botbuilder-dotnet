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
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
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

        private readonly BotFrameworkHttpAdapter _adapter;
        private readonly string _botAppId;
        private readonly ConfigurationChannelProvider _channelProvider;
        private readonly ConfigurationCredentialProvider _credentialsProvider;

        public BotFrameworkHttpSkillsServer(BotFrameworkHttpAdapter adapter, IConfiguration configuration)
        {
            // adapter to use for calling back to channel
            _adapter = adapter;
            _botAppId = configuration.GetValue<string>("MicrosoftAppId");
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

                // TODO: authenticate header here
                //var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, _credentialProvider, _channelProvider, _authConfiguration, _httpClient).ConfigureAwait(false);
                switch (route.Method)
                {
                    // [Route("/v3/conversations")]
                    case ChannelApiMethods.CreateConversation:
                        // TODO: how do we handle this one?
                        throw new NotImplementedException("CreateConversation is not supported for skills");

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
                    case ChannelApiMethods.DeleteActivity:
                        // TODO: ask Tom why we use the value from the route and not from the activity here.
                        await InvokeChannelApiAsync<object>(route.Method, route.Parameters["conversationId"].Value, bot, route.Parameters["activityId"].Value).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/members/{memberId}")]
                    case ChannelApiMethods.DeleteConversationMember:
                        await InvokeChannelApiAsync<object>(route.Method, route.Parameters["conversationId"].Value, bot, route.Parameters["memberId"].Value).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}/members")]
                    case ChannelApiMethods.GetActivityMembers:
                        result = await InvokeChannelApiAsync<ChannelAccount[]>(route.Method, route.Parameters["conversationId"].Value, bot, route.Parameters["activityId"].Value).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/members")]
                    case ChannelApiMethods.GetConversationMembers:
                        result = await InvokeChannelApiAsync<ChannelAccount[]>(route.Method, route.Parameters["conversationId"].Value, bot).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/pagedmembers")]
                    case ChannelApiMethods.GetConversationPagedMembers:
                        var pageSize = string.IsNullOrWhiteSpace(route.Parameters["pageSize"].Value) ? -1 : int.Parse(route.Parameters["pageSize"].Value, CultureInfo.InvariantCulture);
                        result = await InvokeChannelApiAsync<PagedMembersResult>(route.Method, route.Parameters["conversationId"].Value, bot, pageSize, route.Parameters["continuationToken"].Value).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations")]
                    case ChannelApiMethods.GetConversations:
                        // TODO: how do we handle this one?
                        throw new NotImplementedException("GetConversations is not supported for skills");

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
                    case ChannelApiMethods.ReplyToActivity:
                        var replyToActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                        result = await InvokeChannelApiAsync<ResourceResponse>(route.Method, replyToActivity.Conversation.Id, bot, route.Parameters["activityId"].Value, replyToActivity).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/history")]
                    case ChannelApiMethods.SendConversationHistory:
                        var history = HttpHelper.ReadRequest<Transcript>(httpRequest);
                        result = await InvokeChannelApiAsync<ResourceResponse>(route.Method, route.Parameters["conversationId"].Value, bot, history).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities")]
                    case ChannelApiMethods.SendToConversation:
                        var sendToConversationActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                        result = await InvokeChannelApiAsync<ResourceResponse>(route.Method, sendToConversationActivity.Conversation.Id, bot, sendToConversationActivity).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
                    case ChannelApiMethods.UpdateActivity:
                        var updateActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                        result = await InvokeChannelApiAsync<ResourceResponse>(route.Method, updateActivity.Conversation.Id, bot, updateActivity).ConfigureAwait(false);
                        break;

                    // [Route("/v3/conversations/{conversationId}/attachments")]
                    case ChannelApiMethods.UploadAttachment:
                        var uploadAttachment = HttpHelper.ReadRequest<AttachmentData>(httpRequest);
                        result = await InvokeChannelApiAsync<ResourceResponse>(route.Method, route.Parameters["conversationId"].Value, bot, uploadAttachment).ConfigureAwait(false);
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

        private async Task<T> InvokeChannelApiAsync<T>(ChannelApiMethods methods, string conversationId, IBot bot, params object[] args)
        {
            var skillConversation = new SkillConversation(conversationId);

            var channelApiInvokeActivity = Activity.CreateInvokeActivity();
            channelApiInvokeActivity.Name = "ChannelAPI";
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

            var channelApiArgs = new ChannelApiArgs()
            {
                Methods = methods,
                Args = args,
            };
            channelApiInvokeActivity.Value = channelApiArgs;

            // We call our adapter using the BotAppId claim, so turnContext has the bot claims
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                // Adding claims for both Emulator and Channel.
                new Claim(AuthenticationConstants.AudienceClaim, _botAppId),
                new Claim(AuthenticationConstants.AppIdClaim, _botAppId),
                new Claim(AuthenticationConstants.ServiceUrlClaim, skillConversation.ServiceUrl),
            });

            // send up to the bot to process it...
            await _adapter.ProcessActivityAsync(claimsIdentity, (Activity)channelApiInvokeActivity, bot.OnTurnAsync, CancellationToken.None).ConfigureAwait(false);

            // Return the result that was captured in the middleware handler. 
            return (T)channelApiArgs.Result;
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

            public ChannelApiMethods Method { get; set; }
        }

        internal class RouteResult
        {
            public ChannelApiMethods Method { get; set; }

            public GroupCollection Parameters { get; set; }
        }
    }
}
