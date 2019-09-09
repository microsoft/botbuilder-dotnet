// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Helpers
{
    /// <summary>
    /// Helper methods for storing attachments in the channel.
    /// </summary>
    internal static class AttachmentHelper
    {
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
            contentType = null;

            // ContentUrl may contain base64 encoded string of form: "data:[<MIME-type>][;charset=<encoding>][;base64],<data>"
            if (base64Encoded?.TrimStart().StartsWith("data:", StringComparison.OrdinalIgnoreCase) == true)
            {
                var start = base64Encoded.IndexOf("data:", StringComparison.InvariantCulture) + 5;
                var end = base64Encoded.IndexOfAny(";,".ToArray(), start);
                if (end > start)
                {
                    contentType = base64Encoded.Substring(start, end - start).Trim();
                }
            }

            // string off header
            var headerIndex = base64Encoded.IndexOf("base64,", StringComparison.InvariantCulture);
            if (headerIndex >= 0)
            {
                base64Encoded = base64Encoded.Substring(headerIndex + 7).Trim();
            }

            return Convert.FromBase64String(base64Encoded);
        }
    }
}
