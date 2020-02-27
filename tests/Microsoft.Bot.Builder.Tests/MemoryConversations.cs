using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Rest;

namespace Microsoft.Bot.Builder.Tests
{
    public class MemoryConversations : IConversations
    {
        public List<Activity> SentActivities { get; private set; } = new List<Activity>();

        public Task<HttpOperationResponse<ConversationResourceResponse>> CreateConversationWithHttpMessagesAsync(ConversationParameters parameters, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> DeleteActivityWithHttpMessagesAsync(string conversationId, string activityId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> DeleteConversationMemberWithHttpMessagesAsync(string conversationId, string memberId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<ChannelAccount>>> GetActivityMembersWithHttpMessagesAsync(string conversationId, string activityId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<ChannelAccount>>> GetConversationMembersWithHttpMessagesAsync(string conversationId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<ChannelAccount>> GetConversationMemberWithHttpMessagesAsync(string userId, string conversationId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<PagedMembersResult>> GetConversationPagedMembersWithHttpMessagesAsync(string conversationId, int? pageSize = null, string continuationToken = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<ConversationsResult>> GetConversationsWithHttpMessagesAsync(string continuationToken = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<ResourceResponse>> ReplyToActivityWithHttpMessagesAsync(string conversationId, string activityId, Activity activity, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            SentActivities.Add(activity);
            return Task.FromResult(new HttpOperationResponse<ResourceResponse>()
            {
                Body = new ResourceResponse()
                {
                    Id = activity.Id,
                },
            });
        }

        public Task<HttpOperationResponse<ResourceResponse>> SendConversationHistoryWithHttpMessagesAsync(string conversationId, Transcript transcript, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<ResourceResponse>> SendToConversationWithHttpMessagesAsync(string conversationId, Activity activity, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            SentActivities.Add(activity);
            return Task.FromResult(new HttpOperationResponse<ResourceResponse>()
            {
                Body = new ResourceResponse()
                {
                    Id = activity.Id,
                },
            });
        }

        public Task<HttpOperationResponse<ResourceResponse>> UpdateActivityWithHttpMessagesAsync(string conversationId, string activityId, Activity activity, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<ResourceResponse>> UploadAttachmentWithHttpMessagesAsync(string conversationId, AttachmentData attachmentUpload, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
