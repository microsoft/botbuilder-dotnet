// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    /// <summary>
    /// Http Response Mock (used in HttpRequestSequenceMock).
    /// </summary>
    public class HttpResponseMock
    {
        /// <summary>
        /// Types of response content.
        /// </summary>
        public enum ResponseContentType
        {
            /// <summary>
            /// String response.
            /// </summary>
#pragma warning disable CA1720 // Identifier contains type name (by design)
            String,
#pragma warning restore CA1720 // Identifier contains type name

            /// <summary>
            /// Byte array response. The content should be the base64 string of the byte array.
            /// </summary>
            ByteArray
        }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        /// <value>
        /// The content type. Default is String.
        /// </value>
        [DefaultValue(ResponseContentType.String)]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("contentType")]
        public ResponseContentType ContentType { get; set; } = ResponseContentType.String;

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [JsonProperty("content")]
        public object Content { get; set; }
    }
}
