// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Teams
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Rest;
    using Newtonsoft.Json;

    /// <summary>
    /// TeamsOperations operations.
    /// </summary>
    public partial class TeamsOperations : IServiceOperations<TeamsConnectorClient>, ITeamsOperations
    {
        private static volatile RetryParams currentRetryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsOperations"/> class.
        /// </summary>
        /// <param name='client'>
        /// Reference to the service client.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        public TeamsOperations(TeamsConnectorClient client)
        {
            if (client == null)
            {
                throw new System.ArgumentNullException(nameof(client));
            }

            Client = client;
        }

        /// <summary>
        /// Gets a reference to the TeamsConnectorClient.
        /// </summary>
        /// <value>The TeamsConnectorClient.</value>
        public TeamsConnectorClient Client { get; private set; }

        /// <summary>
        /// Fetches channel list for a given team.
        /// </summary>
        /// <remarks>
        /// Fetch the channel list.
        /// </remarks>
        /// <param name='teamId'>
        /// Team Id.
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>
        /// A response object containing the response body and response headers.
        /// </returns>
        public async Task<HttpOperationResponse<ConversationList>> FetchChannelListWithHttpMessagesAsync(string teamId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (teamId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "teamId");
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("teamId", teamId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "FetchChannelList", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "v3/teams/{teamId}/conversations").ToString();
            url = url.Replace("{teamId}", System.Uri.EscapeDataString(teamId));

            return await GetResponseAsync<ConversationList>(url, shouldTrace, invocationId, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches details related to a team.
        /// </summary>
        /// <param name='teamId'>
        /// Team Id.
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>
        /// A response object containing the response body and response headers.
        /// </returns>
        public async Task<HttpOperationResponse<TeamDetails>> FetchTeamDetailsWithHttpMessagesAsync(string teamId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (teamId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "teamId");
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("teamId", teamId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "FetchTeamDetails", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "v3/teams/{teamId}").ToString();
            url = url.Replace("{teamId}", System.Uri.EscapeDataString(teamId));

            return await GetResponseAsync<TeamDetails>(url, shouldTrace, invocationId, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches details related to a meeting.
        /// </summary>
        /// <param name='meetingId'>
        /// Meeting Id, encoded as a BASE64 string.
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>
        /// A response object containing the response body and response headers.
        /// </returns>
        public async Task<HttpOperationResponse<MeetingInfo>> FetchMeetingInfoWithHttpMessagesAsync(string meetingId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (meetingId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "meetingId");
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("meetingId", meetingId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "FetchMeetingInfo", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "v1/meetings/{meetingId}").ToString();
            url = url.Replace("{meetingId}", System.Uri.EscapeDataString(meetingId));

            return await GetResponseAsync<MeetingInfo>(url, shouldTrace, invocationId, customHeaders, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches Teams meeting participant details.
        /// </summary>
        /// <remarks>
        /// Fetches details for a meeting particpant.
        /// </remarks>
        /// <param name='meetingId'>
        /// Teams meeting id.
        /// </param>
        /// <param name='participantId'>
        /// Teams meeting participant id.
        /// </param>
        /// <param name='tenantId'>
        /// Teams meeting tenant id.
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>
        /// A response object containing the response body and response headers.
        /// </returns>
#pragma warning disable CA1801 // Review unused parameters - cannot change without breaking backwards compat.
        public async Task<HttpOperationResponse<TeamsMeetingParticipant>> FetchParticipantWithHttpMessagesAsync(string meetingId, string participantId, string tenantId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
#pragma warning restore CA1801 // Review unused parameters
        {
            if (meetingId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(meetingId));
            }

            if (participantId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(participantId));
            }

            if (tenantId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(tenantId));
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("meetingId", meetingId);
                tracingParameters.Add("participantId", participantId);
                tracingParameters.Add("tenantId", tenantId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "GetParticipant", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "v1/meetings/{meetingId}/participants/{participantId}?tenantId={tenantId}").ToString();
            url = url.Replace("{meetingId}", System.Uri.EscapeDataString(meetingId));
            url = url.Replace("{participantId}", System.Uri.EscapeDataString(participantId));
            url = url.Replace("{tenantId}", System.Uri.EscapeDataString(tenantId));

            return await GetResponseAsync<TeamsMeetingParticipant>(url, shouldTrace, invocationId, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a teams meeting notification.
        /// </summary>
        /// <remarks>
        /// Send a notification to teams meeting particpants.
        /// </remarks>
        /// <param name='meetingId'>
        /// Teams meeting id.
        /// </param>
        /// <param name='notification'>
        /// Teams notification object.
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>
        /// A response object containing the response body and response headers.
        /// </returns>
        public async Task<HttpOperationResponse<MeetingNotificationResponse>> SendMeetingNotificationMessageAsync(string meetingId, MeetingNotificationBase notification, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (meetingId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(meetingId));
            }

            // Tracing
            bool shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("meetingId", meetingId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "SendMeetingNotification", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/", System.StringComparison.InvariantCulture) ? string.Empty : "/")), "v1/meetings/{meetingId}/notification").ToString();
            url = url.Replace("{meetingId}", System.Uri.EscapeDataString(meetingId));
            using var httpRequest = new HttpRequestMessage();
            httpRequest.Method = new HttpMethod("POST");
            httpRequest.RequestUri = new System.Uri(url);

            HttpResponseMessage httpResponse = null;

            // Create HTTP transport objects
#pragma warning disable CA2000 // Dispose objects before losing scope
            var result = new HttpOperationResponse<MeetingNotificationResponse>();
#pragma warning restore CA2000 // Dispose objects before losing scope
            try
            {
                // Set Headers
                if (customHeaders != null)
                {
                    foreach (var header in customHeaders)
                    {
                        if (httpRequest.Headers.Contains(header.Key))
                        {
                            httpRequest.Headers.Remove(header.Key);
                        }

                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // Serialize Request
                string requestContent = null;
                if (notification != null)
                {
                    requestContent = Rest.Serialization.SafeJsonConvert.SerializeObject(notification, Client.SerializationSettings);
                    httpRequest.Content = new StringContent(requestContent, System.Text.Encoding.UTF8);
                    httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
                }

                // Set Credentials
                if (Client.Credentials != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Client.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                }

                // Send Request
                if (shouldTrace)
                {
                    ServiceClientTracing.SendRequest(invocationId, httpRequest);
                }

                cancellationToken.ThrowIfCancellationRequested();
                httpResponse = await Client.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                if (shouldTrace)
                {
                    ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
                }

                HttpStatusCode statusCode = httpResponse.StatusCode;
                cancellationToken.ThrowIfCancellationRequested();
                string responseContent = null;

                // Create Result
                result.Request = httpRequest;
                result.Response = httpResponse;

                if ((int)statusCode == 207)
                {
                    // 207: if the notifications are sent only to parital number of recipients because
                    //    the validation on some recipients’ ids failed or some recipients were not found in the roster.
                    // In this case, SMBA will return the user MRIs of those failed recipients in a format that was given to a bot
                    // (ex: if a bot sent encrypted user MRIs, return encrypted one).

                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    try
                    {
                        result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<MeetingNotificationResponse>(responseContent, Client.DeserializationSettings);
                    }
                    catch (JsonException ex)
                    {
                        if (shouldTrace)
                        {
                            ServiceClientTracing.Error(invocationId, ex);
                        }

                        throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                    }
                }
                else if ((int)statusCode != 202)
                {
                    // 400: when Meeting Notification request payload validation fails. For instance, 
                    //    • Recipients: # of recipients is greater than what the API allows || all of recipients’ user ids were invalid
                    //    • Surface: 
                    //        o Surface list is empty or null 
                    //        o Surface type is invalid 
                    //        o Duplicative surface type exists in one payload
                    // 401: if the bot token is invalid 
                    // 403: if the bot is not allowed to send the notification.
                    //     In this case, the payload should contain more detail error message.
                    //     There can be many reasons: bot disabled by tenant admin, blocked during live site mitigation,
                    //     the bot does not have a correct RSC permission for a specific surface type, etc
                    // 404: if a meeting chat is not found || None of the receipients were found in the roster. 

                    // invalid/unexpected status code
                    var ex = new HttpOperationException($"Operation returned an invalid status code '{statusCode}'");
                    if (httpResponse.Content != null)
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        responseContent = string.Empty;
                    }

                    ex.Request = new HttpRequestMessageWrapper(httpRequest, requestContent);
                    ex.Response = new HttpResponseMessageWrapper(httpResponse, responseContent);
                    if (shouldTrace)
                    {
                        ServiceClientTracing.Error(invocationId, ex);
                    }

                    throw ex;
                }
            }
            finally
            {
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
            }

            if (shouldTrace)
            {
                ServiceClientTracing.Exit(invocationId, result);
            }

            return result;
        }

        /// <summary>
        /// Send a message to a list of Teams members.
        /// </summary>
        /// <param name="activity"> The activity to send. </param>
        /// <param name="teamsMembers"> The list of members. </param>
        /// <param name="tenantId"> The tenant ID. </param>
        /// <param name="customHeaders"> Headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token.  </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <returns>
        /// A response object containing the operation id.
        /// </returns>
        public async Task<HttpOperationResponse<string>> SendMessageToListOfUsersAsync(IActivity activity, List<object> teamsMembers, string tenantId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(activity));
            }

            if (teamsMembers.Count == 0)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(teamsMembers));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(tenantId));
            }

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => SendMessageToListOfUsersWithRetryAsync(activity, teamsMembers, tenantId, customHeaders, cancellationToken),
                retryExceptionHandler: (ex, ct) => HandleThrottlingException(ex, ct)).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Send a message to all the users in a tenant.
        /// </summary>
        /// <param name="activity"> The activity to send. </param>
        /// <param name="tenantId"> The tenant ID. </param>
        /// <param name="customHeaders"> Headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token.  </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <returns>
        /// A response object containing the operation id.
        /// </returns>
        public async Task<HttpOperationResponse<string>> SendMessageToAllUsersInTenantAsync(IActivity activity, string tenantId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(activity));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(tenantId));
            }

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => SendMessageToAllUsersInTenantWithRetryAsync(activity, tenantId, customHeaders, cancellationToken),
                retryExceptionHandler: (ex, ct) => HandleThrottlingException(ex, ct)).ConfigureAwait(false);

            return result;            
        }

        /// <summary>
        /// Send a message to all the users in a team.
        /// </summary>
        /// <param name="activity"> The activity to send. </param>
        /// <param name="teamId"> The team ID. </param>
        /// <param name="tenantId"> The tenant ID. </param>
        /// <param name="customHeaders"> Headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token.  </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <returns>
        /// A response object containing the operation id.
        /// </returns>
        public async Task<HttpOperationResponse<string>> SendMessageToAllUsersInTeamAsync(IActivity activity, string teamId, string tenantId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(activity));
            }

            if (string.IsNullOrEmpty(teamId))
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(teamId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(tenantId));
            }

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => SendMessageToAllUsersInTeamWithRetryAsync(activity, teamId, tenantId, customHeaders, cancellationToken),
                retryExceptionHandler: (ex, ct) => HandleThrottlingException(ex, ct)).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Send a message to a list of Teams channels.
        /// </summary>
        /// <param name="activity"> The activity to send. </param>
        /// <param name="channelsMembers"> The list of channels. </param>
        /// <param name="tenantId"> The tenant ID. </param>
        /// <param name="customHeaders"> Headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token.  </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <returns>
        /// A response object containing the operation id.
        /// </returns>
        public async Task<HttpOperationResponse<string>> SendMessageToListOfChannelsAsync(IActivity activity, List<object> channelsMembers, string tenantId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(activity));
            }

            if (channelsMembers.Count == 0)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(channelsMembers));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(tenantId));
            }

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => SendMessageToListOfChannelsWithRetryAsync(activity, channelsMembers, tenantId, customHeaders, cancellationToken),
                retryExceptionHandler: (ex, ct) => HandleThrottlingException(ex, ct)).ConfigureAwait(false);

            return result;
        }
        
        /// <summary>
        /// Gets the state of an operation.
        /// </summary>
        /// <param name="operationId"> The operationId to get the state of. </param>
        /// <param name="customHeaders"> Headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token. </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <returns>
        /// A response object containing the state and responses of the operation.
        /// </returns>
        public async Task<HttpOperationResponse<BatchOperationState>> GetOperationStateAsync(string operationId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(operationId))
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(operationId));
            }

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => GetOperationStateWithRetryAsync(operationId, customHeaders, cancellationToken),
                retryExceptionHandler: (ex, ct) => HandleThrottlingException(ex, ct)).ConfigureAwait(false);

            return result;
        }

        private static RetryParams HandleThrottlingException(Exception ex, int currentRetryCount)
        {
            if (ex is ThrottleException throttlException)
            {
                return throttlException.RetryParams ?? RetryParams.DefaultBackOff(currentRetryCount);
            }
            else
            {
                return RetryParams.StopRetrying;
            }
        }

        private async Task<HttpOperationResponse<string>> SendMessageToAllUsersInTenantWithRetryAsync(IActivity activity, string tenantId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Tracing
            var shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                var tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("activity", activity);
                tracingParameters.Add("tenantId", tenantId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "SendMessageToAllUsersInTenant", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/", StringComparison.InvariantCulture) ? string.Empty : "/")), "v3/batch/conversation/tenant/").ToString();
            using var httpRequest = new HttpRequestMessage();
            httpRequest.Method = new HttpMethod("POST");
            httpRequest.RequestUri = new Uri(url);

            HttpResponseMessage httpResponse = null;

            // Create HTTP transport objects
#pragma warning disable CA2000 // Dispose objects before losing scope
            var result = new HttpOperationResponse<string>();
#pragma warning restore CA2000 // Dispose objects before losing scope
            try
            {
                // Set Headers
                if (customHeaders != null)
                {
                    foreach (var header in customHeaders)
                    {
                        if (httpRequest.Headers.Contains(header.Key))
                        {
                            httpRequest.Headers.Remove(header.Key);
                        }

                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                var content = new
                {
                    Activity = activity,
                    TenantId = tenantId,
                };

                // Serialize Request
                string requestContent = null;

                if (activity != null)
                {
                    requestContent = Rest.Serialization.SafeJsonConvert.SerializeObject(content);
                    httpRequest.Content = new StringContent(requestContent, System.Text.Encoding.UTF8);
                    httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
                }

                // Set Credentials
                if (Client.Credentials != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Client.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                }

                // Send Request
                if (shouldTrace)
                {
                    ServiceClientTracing.SendRequest(invocationId, httpRequest);
                }

                cancellationToken.ThrowIfCancellationRequested();
                httpResponse = await Client.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                if (shouldTrace)
                {
                    ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
                }

                HttpStatusCode statusCode = httpResponse.StatusCode;
                cancellationToken.ThrowIfCancellationRequested();
                string responseContent = null;

                // Create Result
                result.Request = httpRequest;
                result.Response = httpResponse;

                if ((int)statusCode == 201)
                {
                    // 201: created
                    try
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                        result.Body = responseContent;
                    }
                    catch (JsonException ex)
                    {
                        if (shouldTrace)
                        {
                            ServiceClientTracing.Error(invocationId, ex);
                        }

                        throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                    }
                    finally
                    {
                        // This means the request was successful. We can make our retry policy null.
                        if (currentRetryPolicy != null)
                        {
                            currentRetryPolicy = null;
                        }
                    }
                }
                else if ((int)statusCode == 429)
                {
                    throw new ThrottleException() { RetryParams = currentRetryPolicy };
                }
                else
                {
                    // 400: when request payload validation fails.
                    // 401: if the bot token is invalid 
                    // 403: if bot does not have permission to post messages within Tenant.

                    // invalid/unexpected status code
                    var ex = new HttpOperationException($"Operation returned an invalid status code '{statusCode}'");
                    if (httpResponse.Content != null)
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        responseContent = string.Empty;
                    }

                    ex.Request = new HttpRequestMessageWrapper(httpRequest, requestContent);
                    ex.Response = new HttpResponseMessageWrapper(httpResponse, responseContent);
                    if (shouldTrace)
                    {
                        ServiceClientTracing.Error(invocationId, ex);
                    }

                    throw ex;
                }
            }
            finally
            {
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
            }

            if (shouldTrace)
            {
                ServiceClientTracing.Exit(invocationId, result);
            }

            return result;
        }

        private async Task<HttpOperationResponse<string>> SendMessageToListOfUsersWithRetryAsync(IActivity activity, List<object> teamsMembers, string tenantId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Tracing
            var shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                var tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("activity", activity);
                tracingParameters.Add("teamsMembers", teamsMembers);
                tracingParameters.Add("tenantId", tenantId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "SendMessageToListOfUsers", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/", StringComparison.InvariantCulture) ? string.Empty : "/")), "v3/batch/conversation/users/").ToString();
            using var httpRequest = new HttpRequestMessage();
            httpRequest.Method = new HttpMethod("POST");
            httpRequest.RequestUri = new Uri(url);

            HttpResponseMessage httpResponse = null;

            // Create HTTP transport objects
#pragma warning disable CA2000 // Dispose objects before losing scope
            var result = new HttpOperationResponse<string>();
#pragma warning restore CA2000 // Dispose objects before losing scope
            try
            {
                // Set Headers
                if (customHeaders != null)
                {
                    foreach (var header in customHeaders)
                    {
                        if (httpRequest.Headers.Contains(header.Key))
                        {
                            httpRequest.Headers.Remove(header.Key);
                        }

                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                var content = new
                {
                    Members = teamsMembers,
                    Activity = activity,
                    TenantId = tenantId,
                };

                // Serialize Request
                string requestContent = null;

                if (activity != null)
                {
                    requestContent = Rest.Serialization.SafeJsonConvert.SerializeObject(content);
                    httpRequest.Content = new StringContent(requestContent, System.Text.Encoding.UTF8);
                    httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
                }

                // Set Credentials
                if (Client.Credentials != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Client.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                }

                // Send Request
                if (shouldTrace)
                {
                    ServiceClientTracing.SendRequest(invocationId, httpRequest);
                }

                cancellationToken.ThrowIfCancellationRequested();
                httpResponse = await Client.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                if (shouldTrace)
                {
                    ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
                }

                var statusCode = httpResponse.StatusCode;
                cancellationToken.ThrowIfCancellationRequested();
                string responseContent = null;

                // Create Result
                result.Request = httpRequest;
                result.Response = httpResponse;

                if ((int)statusCode == 201)
                {
                    // 201: created
                    try
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                        result.Body = responseContent;
                    }
                    catch (JsonException ex)
                    {
                        if (shouldTrace)
                        {
                            ServiceClientTracing.Error(invocationId, ex);
                        }

                        throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                    }
                    finally
                    {
                        // This means the request was successful. We can make our retry policy null.
                        if (currentRetryPolicy != null)
                        {
                            currentRetryPolicy = null;
                        }
                    }
                }
                else if ((int)statusCode == 429)
                {
                    throw new ThrottleException() { RetryParams = currentRetryPolicy };
                }
                else
                {
                    // 400: when request payload validation fails.
                    // 401: if the bot token is invalid. 
                    // 403: if bot does not have permission to post messages within Tenant.

                    // invalid/unexpected status code
                    var ex = new HttpOperationException($"Operation returned an invalid status code '{statusCode}'");
                    if (httpResponse.Content != null)
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        responseContent = string.Empty;
                    }

                    ex.Request = new HttpRequestMessageWrapper(httpRequest, requestContent);
                    ex.Response = new HttpResponseMessageWrapper(httpResponse, responseContent);
                    if (shouldTrace)
                    {
                        ServiceClientTracing.Error(invocationId, ex);
                    }

                    throw ex;
                }
            }
            finally
            {
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
            }

            if (shouldTrace)
            {
                ServiceClientTracing.Exit(invocationId, result);
            }

            return result;
        }

        private async Task<HttpOperationResponse<string>> SendMessageToListOfChannelsWithRetryAsync(IActivity activity, List<object> channelsMembers, string tenantId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Tracing
            var shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                var tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("activity", activity);
                tracingParameters.Add("channelsMembers", channelsMembers);
                tracingParameters.Add("tenantId", tenantId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "SendMessageToListOfChannels", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/", StringComparison.InvariantCulture) ? string.Empty : "/")), "v3/batch/conversation/channels/").ToString();
            using var httpRequest = new HttpRequestMessage();
            httpRequest.Method = new HttpMethod("POST");
            httpRequest.RequestUri = new Uri(url);

            HttpResponseMessage httpResponse = null;

            // Create HTTP transport objects
#pragma warning disable CA2000 // Dispose objects before losing scope
            var result = new HttpOperationResponse<string>();
#pragma warning restore CA2000 // Dispose objects before losing scope
            try
            {
                // Set Headers
                if (customHeaders != null)
                {
                    foreach (var header in customHeaders)
                    {
                        if (httpRequest.Headers.Contains(header.Key))
                        {
                            httpRequest.Headers.Remove(header.Key);
                        }

                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                var content = new
                {
                    Members = channelsMembers,
                    Activity = activity,
                    TenantId = tenantId,
                };

                // Serialize Request
                string requestContent = null;

                if (activity != null)
                {
                    requestContent = Rest.Serialization.SafeJsonConvert.SerializeObject(content);
                    httpRequest.Content = new StringContent(requestContent, System.Text.Encoding.UTF8);
                    httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
                }

                // Set Credentials
                if (Client.Credentials != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Client.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                }

                // Send Request
                if (shouldTrace)
                {
                    ServiceClientTracing.SendRequest(invocationId, httpRequest);
                }

                cancellationToken.ThrowIfCancellationRequested();
                httpResponse = await Client.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                if (shouldTrace)
                {
                    ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
                }

                var statusCode = httpResponse.StatusCode;
                cancellationToken.ThrowIfCancellationRequested();
                string responseContent = null;

                // Create Result
                result.Request = httpRequest;
                result.Response = httpResponse;

                if ((int)statusCode == 201)
                {
                    // 201: created
                    try
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                        result.Body = responseContent;
                    }
                    catch (JsonException ex)
                    {
                        if (shouldTrace)
                        {
                            ServiceClientTracing.Error(invocationId, ex);
                        }

                        throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                    }
                    finally
                    {
                        // This means the request was successful. We can make our retry policy null.
                        if (currentRetryPolicy != null)
                        {
                            currentRetryPolicy = null;
                        }
                    }
                }
                else if ((int)statusCode == 429)
                {
                    throw new ThrottleException() { RetryParams = currentRetryPolicy };
                }
                else
                {
                    // 400: when request payload validation fails.
                    // 401: if the bot token is invalid. 
                    // 403: if bot does not have permission to post messages within Tenant.

                    // invalid/unexpected status code
                    var ex = new HttpOperationException($"Operation returned an invalid status code '{statusCode}'");
                    if (httpResponse.Content != null)
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        responseContent = string.Empty;
                    }

                    ex.Request = new HttpRequestMessageWrapper(httpRequest, requestContent);
                    ex.Response = new HttpResponseMessageWrapper(httpResponse, responseContent);
                    if (shouldTrace)
                    {
                        ServiceClientTracing.Error(invocationId, ex);
                    }

                    throw ex;
                }
            }
            finally
            {
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
            }

            if (shouldTrace)
            {
                ServiceClientTracing.Exit(invocationId, result);
            }

            return result;
        }

        private async Task<HttpOperationResponse<string>> SendMessageToAllUsersInTeamWithRetryAsync(IActivity activity, string teamId, string tenantId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Tracing
            var shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                var tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("activity", activity);
                tracingParameters.Add("teamId", teamId);
                tracingParameters.Add("tenantId", tenantId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "SendMessageToAllUsersInTeam", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/", StringComparison.InvariantCulture) ? string.Empty : "/")), "v3/batch/conversation/team/").ToString();
            using var httpRequest = new HttpRequestMessage();
            httpRequest.Method = new HttpMethod("POST");
            httpRequest.RequestUri = new Uri(url);

            HttpResponseMessage httpResponse = null;

            // Create HTTP transport objects
#pragma warning disable CA2000 // Dispose objects before losing scope
            var result = new HttpOperationResponse<string>();
#pragma warning restore CA2000 // Dispose objects before losing scope
            try
            {
                // Set Headers
                if (customHeaders != null)
                {
                    foreach (var header in customHeaders)
                    {
                        if (httpRequest.Headers.Contains(header.Key))
                        {
                            httpRequest.Headers.Remove(header.Key);
                        }

                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                var content = new
                {
                    Activity = activity,
                    TeamId = teamId,
                    TenantId = tenantId,
                };

                // Serialize Request
                string requestContent = null;

                if (activity != null)
                {
                    requestContent = Rest.Serialization.SafeJsonConvert.SerializeObject(content);
                    httpRequest.Content = new StringContent(requestContent, System.Text.Encoding.UTF8);
                    httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
                }

                // Set Credentials
                if (Client.Credentials != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Client.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                }

                // Send Request
                if (shouldTrace)
                {
                    ServiceClientTracing.SendRequest(invocationId, httpRequest);
                }

                cancellationToken.ThrowIfCancellationRequested();
                httpResponse = await Client.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                if (shouldTrace)
                {
                    ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
                }

                HttpStatusCode statusCode = httpResponse.StatusCode;
                cancellationToken.ThrowIfCancellationRequested();
                string responseContent = null;

                // Create Result
                result.Request = httpRequest;
                result.Response = httpResponse;

                if ((int)statusCode == 201)
                {
                    // 201: created
                    try
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                        result.Body = responseContent;
                    }
                    catch (JsonException ex)
                    {
                        if (shouldTrace)
                        {
                            ServiceClientTracing.Error(invocationId, ex);
                        }

                        throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                    }
                    finally
                    {
                        // This means the request was successfull. We can make our retry policy null.
                        if (currentRetryPolicy != null)
                        {
                            currentRetryPolicy = null;
                        }
                    }
                }
                else if ((int)statusCode == 429)
                {
                    throw new ThrottleException() { RetryParams = currentRetryPolicy };
                }
                else
                {
                    // 400: when request payload validation fails.
                    // 401: if the bot token is invalid 
                    // 403: if bot does not have permission to post messages within Tenant.
                    // 404: when Team is not found.

                    // invalid/unexpected status code
                    var ex = new HttpOperationException($"Operation returned an invalid status code '{statusCode}'");
                    if (httpResponse.Content != null)
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        responseContent = string.Empty;
                    }

                    ex.Request = new HttpRequestMessageWrapper(httpRequest, requestContent);
                    ex.Response = new HttpResponseMessageWrapper(httpResponse, responseContent);
                    if (shouldTrace)
                    {
                        ServiceClientTracing.Error(invocationId, ex);
                    }

                    throw ex;
                }
            }
            finally
            {
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
            }

            if (shouldTrace)
            {
                ServiceClientTracing.Exit(invocationId, result);
            }

            return result;
        }

        private async Task<HttpOperationResponse<BatchOperationState>> GetOperationStateWithRetryAsync(string operationId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Tracing
            var shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;
            if (shouldTrace)
            {
                invocationId = ServiceClientTracing.NextInvocationId.ToString(CultureInfo.InvariantCulture);
                var tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("operationId", operationId);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, "GetOperationState", tracingParameters);
            }

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/", StringComparison.InvariantCulture) ? string.Empty : "/")), "v3/batch/conversation/{operationId}").ToString();
            url = url.Replace("{operationId}", System.Uri.EscapeDataString(operationId));
            using var httpRequest = new HttpRequestMessage();
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = new Uri(url);

            HttpResponseMessage httpResponse = null;

            // Create HTTP transport objects
#pragma warning disable CA2000 // Dispose objects before losing scope
            var result = new HttpOperationResponse<BatchOperationState>();
#pragma warning restore CA2000 // Dispose objects before losing scope
            try
            {
                // Set Headers
                if (customHeaders != null)
                {
                    foreach (var header in customHeaders)
                    {
                        if (httpRequest.Headers.Contains(header.Key))
                        {
                            httpRequest.Headers.Remove(header.Key);
                        }

                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // Set Credentials
                if (Client.Credentials != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Client.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                }

                // Send Request
                if (shouldTrace)
                {
                    ServiceClientTracing.SendRequest(invocationId, httpRequest);
                }

                cancellationToken.ThrowIfCancellationRequested();
                httpResponse = await Client.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                if (shouldTrace)
                {
                    ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
                }

                var statusCode = httpResponse.StatusCode;
                cancellationToken.ThrowIfCancellationRequested();
                string responseContent = null;

                // Create Result
                result.Request = httpRequest;
                result.Response = httpResponse;

                if ((int)statusCode == 200)
                {
                    // 200: OK

                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    try
                    {
                        result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<BatchOperationState>(responseContent, Client.DeserializationSettings);
                    }
                    catch (JsonException ex)
                    {
                        if (shouldTrace)
                        {
                            ServiceClientTracing.Error(invocationId, ex);
                        }

                        throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                    }
                    finally
                    {
                        // This means the request was successful. We can make our retry policy null.
                        if (currentRetryPolicy != null)
                        {
                            currentRetryPolicy = null;
                        }
                    }
                }
                else if ((int)statusCode == 429)
                {
                    throw new ThrottleException() { RetryParams = currentRetryPolicy };
                }
                else
                {
                    // 400: for requests with invalid operationId (Which should be of type GUID)

                    // invalid/unexpected status code
                    var ex = new HttpOperationException($"Operation returned an invalid status code '{statusCode}'");
                    if (httpResponse.Content != null)
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        responseContent = string.Empty;
                    }

                    ex.Request = new HttpRequestMessageWrapper(httpRequest, operationId);
                    ex.Response = new HttpResponseMessageWrapper(httpResponse, responseContent);
                    if (shouldTrace)
                    {
                        ServiceClientTracing.Error(invocationId, ex);
                    }

                    throw ex;
                }
            }
            finally
            {
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
            }

            if (shouldTrace)
            {
                ServiceClientTracing.Exit(invocationId, result);
            }

            return result;
        }

        private async Task<HttpOperationResponse<T>> GetResponseAsync<T>(string url, bool shouldTrace, string invocationId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = new System.Uri(url);

            // Set Headers
            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    if (httpRequest.Headers.Contains(header.Key))
                    {
                        httpRequest.Headers.Remove(header.Key);
                    }

                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Serialize Request
            string requestContent = null;

            // Set Credentials
            if (Client.Credentials != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Client.Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            }

            // Send Request
            if (shouldTrace)
            {
                ServiceClientTracing.SendRequest(invocationId, httpRequest);
            }

            cancellationToken.ThrowIfCancellationRequested();
            httpResponse = await Client.HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            if (shouldTrace)
            {
                ServiceClientTracing.ReceiveResponse(invocationId, httpResponse);
            }

            HttpStatusCode statusCode = httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string responseContent = null;
            if ((int)statusCode != 200)
            {
                var ex = new HttpOperationException($"Operation returned an invalid status code '{statusCode}'");
                if (httpResponse.Content != null)
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    responseContent = string.Empty;
                }

                ex.Request = new HttpRequestMessageWrapper(httpRequest, requestContent);
                ex.Response = new HttpResponseMessageWrapper(httpResponse, responseContent);
                if (shouldTrace)
                {
                    ServiceClientTracing.Error(invocationId, ex);
                }

                httpRequest.Dispose();
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }

                throw ex;
            }

            // Create Result
            var result = new HttpOperationResponse<T>();
            result.Request = httpRequest;
            result.Response = httpResponse;

            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result.Body = Rest.Serialization.SafeJsonConvert.DeserializeObject<T>(responseContent, Client.DeserializationSettings);
                }
                catch (JsonException ex)
                {
                    httpRequest.Dispose();
                    if (httpResponse != null)
                    {
                        httpResponse.Dispose();
                    }

                    throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                }
            }

            if (shouldTrace)
            {
                ServiceClientTracing.Exit(invocationId, result);
            }

            return result;
        }
    }
}
