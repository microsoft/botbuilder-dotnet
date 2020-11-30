// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Rest;

    /// <summary>
    /// Attachments operations.
    /// </summary>
    public partial interface IAttachments
    {
        /// <summary>
        /// GetAttachmentInfo.
        /// </summary>
        /// <remarks>
        /// Get AttachmentInfo structure describing the attachment views.
        /// </remarks>
        /// <param name='attachmentId'>
        /// attachment id.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task<HttpOperationResponse<AttachmentInfo>> GetAttachmentInfoWithHttpMessagesAsync(string attachmentId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// GetAttachment.
        /// </summary>
        /// <remarks>
        /// Get the named view as binary content.
        /// </remarks>
        /// <param name='attachmentId'>
        /// attachment id.
        /// </param>
        /// <param name='viewId'>
        /// View id from attachmentInfo.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task<HttpOperationResponse<Stream>> GetAttachmentWithHttpMessagesAsync(string attachmentId, string viewId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
