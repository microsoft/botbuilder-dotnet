// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Net;
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
            ByteArray,

            /// <summary>
            /// String content with GZip format.
            /// </summary>
            GZipString
        }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code. Default is OK(200).
        /// </value>
        [DefaultValue(HttpStatusCode.OK)]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("statusCode")]
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        /// <summary>
        /// Gets or sets the reason phrase.
        /// </summary>
        /// <value>
        /// The reason phrase.
        /// </value>
        [JsonProperty("reasonPhrase")]
        public string ReasonPhrase { get; set; }

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
