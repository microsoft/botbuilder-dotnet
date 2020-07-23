// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
<<<<<<< HEAD
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Actions.HttpRequest;
=======
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    public class HttpResponseMock
    {
<<<<<<< HEAD
        public enum ContentTypes
=======
        public enum ResponseContentType
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8
        {
            /// <summary>
            /// String response.
            /// </summary>
<<<<<<< HEAD
            String,
=======
#pragma warning disable CA1720 // Identifier contains type name (by design)
            String,
#pragma warning restore CA1720 // Identifier contains type name
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8

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
<<<<<<< HEAD
        [DefaultValue(ContentTypes.String)]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("contentType")]
        public ContentTypes ContentType { get; set; } = ContentTypes.String;
=======
        [DefaultValue(ResponseContentType.String)]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("contentType")]
        public ResponseContentType ContentType { get; set; } = ResponseContentType.String;
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8

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
