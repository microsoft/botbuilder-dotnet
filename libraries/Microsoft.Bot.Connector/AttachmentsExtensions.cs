// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Extension methods for Attachments.
    /// </summary>
    public static partial class AttachmentsExtensions
    {
            /// <summary>
            /// GetAttachmentInfo.
            /// </summary>
            /// <remarks>
            /// Get AttachmentInfo structure describing the attachment views.
            /// </remarks>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='attachmentId'>
            /// attachment id.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            /// <returns>The AttachmentInfo.</returns>
            public static async Task<AttachmentInfo> GetAttachmentInfoAsync(this IAttachments operations, string attachmentId, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var result = await operations.GetAttachmentInfoWithHttpMessagesAsync(attachmentId, null, cancellationToken).ConfigureAwait(false))
                {
                    return result.Body;
                }
            }

            /// <summary>
            /// GetAttachment.
            /// </summary>
            /// <remarks>
            /// Get the named view as binary content.
            /// </remarks>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='attachmentId'>
            /// attachment id.
            /// </param>
            /// <param name='viewId'>
            /// View id from attachmentInfo.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            /// <returns>The attachment as a Stream.</returns>
            public static async Task<Stream> GetAttachmentAsync(this IAttachments operations, string attachmentId, string viewId, CancellationToken cancellationToken = default(CancellationToken))
            {
                var result = await operations.GetAttachmentWithHttpMessagesAsync(attachmentId, viewId, null, cancellationToken).ConfigureAwait(false);
                result.Request.Dispose();
                return result.Body;
            }
    }
}
