// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Addition helper code for Attachments
    /// </summary>
    public partial class Attachments
    {
        /// <summary>
        /// The attachment code uses this client. Ideally, this would be passed in or set via a DI system to 
        /// allow developer control over behavior / headers / timesouts and such. Unfortunatly this is buried
        /// pretty deep, the static solution used here is much cleaner. If this becomes an issue we could
        /// consider circling back and exposing developer control over this HttpClient. 
        /// </summary>
        /// <remarks>
        /// Relativly few bots use attachments, so rather than paying the startup cost, this is
        /// a <see cref="Lazy{T}"/> simply to avoid paying a static initialization penalty for every bot. 
        /// </remarks>
        private static Lazy<HttpClient> _httpClient = new Lazy<HttpClient>(); 

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
            var url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? string.Empty : "/")), "v3/attachments/{attachmentId}/views/{viewId}").ToString();
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
            return _httpClient.Value.GetStreamAsync(GetAttachmentUri(attachmentId, viewId));
        }
    }
}
