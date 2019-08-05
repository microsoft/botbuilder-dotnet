// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Helpers
{
    /// <summary>
    /// Helper methods for storing attachments in the channel.
    /// </summary>
    internal static class AttachmentHelper
    {
        public static bool IsValidAttachmentData(AttachmentData attachmentData)
        {
            if (attachmentData == null || attachmentData.Type == null || attachmentData.OriginalBase64 == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the content param can be turned into an Uri with http or https scheme.
        /// </summary>
        /// <param name="content">Content object to check if it is an URL.</param>
        /// <returns>Ture or False.</returns>
        public static bool IsUrl(object content)
        {
            return Uri.TryCreate(content as string, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static byte[] DecodeBase64String(string base64Encoded, out string contentType)
        {
            contentType = GetContentTypeFromDataUrl(base64Encoded);

            if (string.IsNullOrWhiteSpace(base64Encoded))
            {
                return null;
            }

            // string off header
            var start = base64Encoded.IndexOf("base64,", StringComparison.InvariantCulture);
            if (start >= 0)
            {
                base64Encoded = base64Encoded.Substring(start + 7).Trim();
            }

            // base64 length must be multiple of 4
            if ((base64Encoded.Length % 4) != 0)
            {
                return null;
            }

            try
            {
                return Convert.FromBase64String(base64Encoded);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private static string GetContentTypeFromDataUrl(string dataUrl)
        {
            // ContentUrl may contain base64 encoded string of form: "data:[<MIME-type>][;charset=<encoding>][;base64],<data>"
            if (dataUrl?.TrimStart().StartsWith("data:", StringComparison.OrdinalIgnoreCase) == true)
            {
                var start = dataUrl.IndexOf("data:", StringComparison.InvariantCulture) + 5;
                var end = dataUrl.IndexOfAny(";,".ToArray(), start);
                if (end > start)
                {
                    return dataUrl.Substring(start, end - start).Trim();
                }
            }

            return null;
        }
    }
}
