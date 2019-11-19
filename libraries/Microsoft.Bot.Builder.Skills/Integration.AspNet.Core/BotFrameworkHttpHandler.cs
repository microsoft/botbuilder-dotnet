// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Skills
{
    internal delegate Task<object> RouteAction(ChannelServiceHandler handler, ClaimsIdentity claimsIdentity, HttpRequest httpRequest, GroupCollection parameters, CancellationToken cancellationToken);

    public class BotFrameworkHttpHandler
    {
        private static readonly ChannelRoute[] _routes =
        {
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetActivityMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>.*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    return await handler.OnGetActivityMembersAsync(claimsIdentity, parameters["conversationId"].Value, parameters["activityId"].Value, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.SendConversationHistory,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/history", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var history = HttpHelper.ReadRequest<Transcript>(httpRequest);
                    return await handler.OnSendConversationHistoryAsync(claimsIdentity, parameters["conversationId"].Value, history, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.ReplyToActivity,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var replyToActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                    return await handler.OnReplyToActivityAsync(claimsIdentity, replyToActivity.Conversation.Id, parameters["activityId"].Value, replyToActivity, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.UpdateActivity,
                Pattern = new Regex(@"/PUT:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var updateActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                    return await handler.OnUpdateActivityAsync(claimsIdentity, parameters["conversationId"].Value, parameters["activityId"].Value, updateActivity, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.DeleteActivity,
                Pattern = new Regex(@"/DELETE:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities/(?<activityId>[^\s/]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    await handler.OnDeleteActivityAsync(claimsIdentity, parameters["conversationId"].Value, parameters["activityId"].Value, cancellationToken).ConfigureAwait(false);
                    return null;
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.SendToConversation,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/activities", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var sendToConversationActivity = HttpHelper.ReadRequest<Activity>(httpRequest);
                    return await handler.OnSendToConversationAsync(claimsIdentity, parameters["conversationId"].Value, sendToConversationActivity, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.DeleteConversationMember,
                Pattern = new Regex(@"/DELETE:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/members/(?<memberId>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    await handler.OnDeleteConversationMemberAsync(claimsIdentity, parameters["conversationId"].Value, parameters["memberId"].Value, cancellationToken).ConfigureAwait(false);
                    return null;
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.UploadAttachment,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/attachments", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var uploadAttachment = HttpHelper.ReadRequest<AttachmentData>(httpRequest);
                    return await handler.OnUploadAttachmentAsync(claimsIdentity, parameters["conversationId"].Value, uploadAttachment, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversationMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/members", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    return await handler.OnGetConversationMembersAsync(claimsIdentity, parameters["conversationId"].Value, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversationPagedMembers,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/(?<conversationId>[^\s/]*)/pagedmember", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var pageSize = string.IsNullOrWhiteSpace(parameters["pageSize"].Value) ? -1 : int.Parse(parameters["pageSize"].Value, CultureInfo.InvariantCulture);
                    return await handler.OnGetConversationPagedMembersAsync(claimsIdentity, parameters["conversationId"].Value, pageSize, parameters["continuationToken"].Value, cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.GetConversations,
                Pattern = new Regex(@"/GET:(?<path>.*)/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    return await handler.OnGetConversationsAsync(claimsIdentity, parameters["conversationId"].Value, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            },
            new ChannelRoute
            {
                Method = ChannelApiMethods.CreateConversation,
                Pattern = new Regex(@"/POST:(?<path>.*)/v3/conversations/", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Action = async (handler, claimsIdentity, httpRequest, parameters, cancellationToken) =>
                {
                    var conversationParameters = HttpHelper.ReadRequest<ConversationParameters>(httpRequest);
                    return await handler.OnCreateConversationAsync(claimsIdentity, parameters["conversationId"].Value, conversationParameters, cancellationToken).ConfigureAwait(false);
                }
            }
        };

        private readonly AuthenticationConfiguration _authConfiguration;
        private readonly IChannelProvider _channelProvider;
        private readonly ICredentialProvider _credentialProvider;
        private readonly ChannelServiceHandler _handler;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpHandler"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="handler">A <see cref="ChannelServiceHandler"/> that will handle the incoming request.</param>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <exception cref="ArgumentNullException">throw ArgumentNullException.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the Use(<see cref="IMiddleware"/>) method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkHttpHandler(
            ChannelServiceHandler handler,
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfig,
            IChannelProvider channelProvider = null)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _authConfiguration = authConfig ?? throw new ArgumentNullException(nameof(authConfig));
            _channelProvider = channelProvider;
        }

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken = default)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            object result = null;
            var statusCode = (int)HttpStatusCode.OK;
            try
            {
                // grab the auth header from the inbound http request
                var authHeader = httpRequest.Headers["Authorization"];
                var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, _credentialProvider, _channelProvider, "unknown", _authConfiguration).ConfigureAwait(false);

                var route = GetRoute(httpRequest);
                if (route == null)
                {
                    httpResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                // Execute the route action
                result = await route.Action.Invoke(_handler, claimsIdentity, httpRequest, route.Parameters, cancellationToken).ConfigureAwait(false);
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
