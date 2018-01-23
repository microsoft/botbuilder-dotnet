// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector
{
    public partial class Attachments
    {
        /// <summary>
        /// Get the URI of an attachment view
        /// </summary>
        /// <param name="attachmentId"></param>
        /// <param name="viewId">default is "original"</param>
        /// <returns>uri</returns>
        public string GetAttachmentUri(string attachmentId, string viewId = "original")
        {
            if (attachmentId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "attachmentId");
            }

            // Construct URL
            var _baseUrl = this.Client.BaseUri.AbsoluteUri;
            var url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "v3/attachments/{attachmentId}/views/{viewId}").ToString();
            url = url.Replace("{attachmentId}", Uri.EscapeDataString(attachmentId));
            url = url.Replace("{viewId}", Uri.EscapeDataString(viewId));
            return url;
        }

        /// <summary>
        /// Get the given attachmentid view as a stream
        /// </summary>
        /// <param name="attachmentId">attachmentid</param>
        /// <param name="viewId">view to get (default:original)</param>
        /// <returns>stream of attachment</returns>
        public Task<Stream> GetAttachmentStreamAsync(string attachmentId, string viewId = "original")
        {
            using (HttpClient client = new HttpClient())
            {
                return client.GetStreamAsync(GetAttachmentUri(attachmentId, viewId));
            }
        }
    }
}
