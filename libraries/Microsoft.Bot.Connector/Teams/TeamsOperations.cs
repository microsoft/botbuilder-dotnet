// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Teams
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Reflection;
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

            var invocationId = TraceActivity("FetchChannelList", new { teamId }, cancellationToken);

            // Construct URL
            var url = "v3/teams/{teamId}/conversations";
            url = url.Replace("{teamId}", Uri.EscapeDataString(teamId));

            return await GetResponseAsync<ConversationList>(url, "GET", invocationId, cancellationToken: cancellationToken).ConfigureAwait(false);
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

            var invocationId = TraceActivity("FetchTeamDetails", new { teamId }, cancellationToken);

            // Construct URL
            var url = "v3/teams/{teamId}";
            url = url.Replace("{teamId}", Uri.EscapeDataString(teamId));

            return await GetResponseAsync<TeamDetails>(url, "GET", invocationId, customHeaders: customHeaders, cancellationToken: cancellationToken).ConfigureAwait(false);
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

            var invocationId = TraceActivity("FetchMeetingInfo", new { meetingId }, cancellationToken);

            // Construct URL
            var url = "v1/meetings/{meetingId}";
            url = url.Replace("{meetingId}", System.Uri.EscapeDataString(meetingId));

            return await GetResponseAsync<MeetingInfo>(url, "GET", invocationId, customHeaders: customHeaders, cancellationToken: cancellationToken).ConfigureAwait(false);
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

            var content = new
            {
                meetingId,
                participantId,
                tenantId,
            };

            var invocationId = TraceActivity("FetchParticipant", content, cancellationToken);

            // Construct URL
            var url = "v1/meetings/{meetingId}/participants/{participantId}?tenantId={tenantId}";
            url = url.Replace("{meetingId}", System.Uri.EscapeDataString(meetingId));
            url = url.Replace("{participantId}", System.Uri.EscapeDataString(participantId));
            url = url.Replace("{tenantId}", System.Uri.EscapeDataString(tenantId));

            return await GetResponseAsync<TeamsMeetingParticipant>(url, "GET", invocationId, customHeaders: customHeaders, cancellationToken: cancellationToken).ConfigureAwait(false); 
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

            var invocationId = TraceActivity("SendMeetingNotification", new { meetingId }, cancellationToken);           

            // Construct URL
            var url = "v1/meetings/{meetingId}/notification";
            url = url.Replace("{meetingId}", Uri.EscapeDataString(meetingId));

            return await GetResponseAsync<MeetingNotificationResponse>(url, "POST", invocationId, content: notification, customHeaders: customHeaders, cancellationToken: cancellationToken).ConfigureAwait(false);
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

            var content = new
            {
                Members = teamsMembers,
                Activity = activity,
                TenantId = tenantId,
            };

            var invocationId = TraceActivity("SendMessageToListOfUsers", content, cancellationToken);
            var apiUrl = "v3/batch/conversation/users/";

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => GetResponseAsync<string>(apiUrl, "POST", invocationId, content: content, customHeaders: customHeaders, cancellationToken: cancellationToken),
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

            var content = new
            {
                Activity = activity,
                TenantId = tenantId,
            };

            var invocationId = TraceActivity("SendMessageToAllUsersInTenant", content, cancellationToken);
            var apiUrl = "v3/batch/conversation/tenant/";

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => GetResponseAsync<string>(apiUrl, "POST", invocationId, content: content, customHeaders: customHeaders, cancellationToken: cancellationToken),
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

            var content = new
            {
                Activity = activity,
                TeamId = teamId,
                TenantId = tenantId,
            };
     
            var invocationId = TraceActivity("SendMessageToAllUsersInTeam", content, cancellationToken);
            var apiUrl = "v3/batch/conversation/team/";

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => GetResponseAsync<string>(apiUrl, "POST", invocationId, content: content, customHeaders: customHeaders, cancellationToken: cancellationToken),
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

            var content = new
            {
                Members = channelsMembers,
                Activity = activity,
                TenantId = tenantId,
            };

            var invocationId = TraceActivity("SendMessageToListOfChannels", content, cancellationToken);
            var apiUrl = "v3/batch/conversation/channels/";

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => GetResponseAsync<string>(apiUrl, "POST", invocationId, content: content, customHeaders: customHeaders, cancellationToken: cancellationToken),
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

            var invocationId = TraceActivity("GetOperationState", new { OperationId = operationId }, cancellationToken);
            var apiUrl = "v3/batch/conversation/{operationId}";
            apiUrl = apiUrl.Replace("{operationId}", Uri.EscapeDataString(operationId));

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => GetResponseAsync<BatchOperationState>(apiUrl, "GET", invocationId, customHeaders: customHeaders, cancellationToken: cancellationToken),
                retryExceptionHandler: (ex, ct) => HandleThrottlingException(ex, ct)).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Gets the failed entries of a batch operation with error code and message.
        /// </summary>
        /// <param name="operationId"> The operationId to get the failed entries of. </param>
        /// <param name="customHeaders"> Headers that will be added to request. </param>
        /// <param name="continuationToken"> The continuation token. </param>
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
        public async Task<HttpOperationResponse<BatchFailedEntriesResponse>> GetPagedFailedEntriesAsync(string operationId, Dictionary<string, List<string>> customHeaders = null, string continuationToken = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(operationId))
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(operationId));
            }

            var invocationId = TraceActivity("GetPagedFailedEntries", new { OperationId = operationId }, cancellationToken);
            var apiUrl = "v3/batch/conversation/failedentries/{operationId}";
            apiUrl = apiUrl.Replace("{operationId}", Uri.EscapeDataString(operationId));

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => GetResponseAsync<BatchFailedEntriesResponse>(apiUrl, "GET", invocationId, continuationToken: continuationToken, customHeaders: customHeaders, cancellationToken: cancellationToken),
                retryExceptionHandler: (ex, ct) => HandleThrottlingException(ex, ct)).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Cancels a batch operation by its id.
        /// </summary>
        /// <param name="operationId"> The id of the operation to cancel. </param>
        /// <param name="customHeaders"> Headers that will be added to request. </param>
        /// <param name='cancellationToken'> The cancellation token. </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown when an input value does not match the expected data type, range or pattern.
        /// </exception>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public async Task<HttpOperationResponse> CancelOperationAsync(string operationId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(operationId))
            {
                throw new ValidationException(ValidationRules.CannotBeNull, nameof(operationId));
            }

            var invocationId = TraceActivity("CancelOperation", new { OperationId = operationId }, cancellationToken);     
            var apiUrl = "v3/batch/conversation/{operationId}";
            apiUrl = apiUrl.Replace("{operationId}", Uri.EscapeDataString(operationId));

            // In case of throttling, it will retry the operation with default values (10 retries every 50 miliseconds).
            var result = await RetryAction.RunAsync(
                task: () => GetResponseAsync<BatchOperationState>(apiUrl, "DELETE", invocationId, customHeaders: customHeaders, cancellationToken: cancellationToken),
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

        private async Task<HttpOperationResponse<T>> GetResponseAsync<T>(string apiUrl, string httpMethod, string invocationId, object content = null, Dictionary<string, List<string>> customHeaders = null, string continuationToken = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var shouldTrace = invocationId != null;             

            // Construct URL
            var baseUrl = Client.BaseUri.AbsoluteUri;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/", StringComparison.InvariantCulture) ? string.Empty : "/")), apiUrl).ToString();
            using var httpRequest = new HttpRequestMessage();
            httpRequest.Method = new HttpMethod(httpMethod);

            if (continuationToken != null)
            {
                url += "?" + string.Join("&", string.Format(CultureInfo.InvariantCulture, "continuationToken={0}", Uri.EscapeDataString(continuationToken)));
            }

            httpRequest.RequestUri = new Uri(url);
            HttpResponseMessage httpResponse = null;

            // Create HTTP transport objects
#pragma warning disable CA2000 // Dispose objects before losing scope
            var result = new HttpOperationResponse<T>();
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
                if (content != null)
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

                if (httpResponse.IsSuccessStatusCode)
                {
                    //200: ok
                    //201: created
                    try
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                        if (typeof(T) == typeof(string))
                        {
                            responseContent = JsonConvert.SerializeObject(responseContent, Client.SerializationSettings);
                        }

                        result.Body = JsonConvert.DeserializeObject<T>(responseContent, Client.DeserializationSettings);
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
                    // 404: the resource couldn't be found
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

        private string TraceActivity(string operationName, object content, CancellationToken cancellationToken)
        {
            var shouldTrace = ServiceClientTracing.IsEnabled;
            string invocationId = null;

            if (shouldTrace)
            {
                var tracingParameters = new Dictionary<string, object>();
                foreach (PropertyInfo prop in content.GetType().GetProperties())
                {
                    tracingParameters.Add(prop.Name, prop.GetValue(content));
                }

                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(invocationId, this, operationName, tracingParameters);
            }

            return invocationId;
        }
    }
}
