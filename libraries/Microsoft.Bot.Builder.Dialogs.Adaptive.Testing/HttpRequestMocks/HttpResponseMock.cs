// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Actions.HttpRequest;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    public class HttpResponseMock
    {
        public enum ContentTypes
        {
            /// <summary>
            /// String response.
            /// </summary>
            String,

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
        [DefaultValue(ContentTypes.String)]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("contentType")]
        public ContentTypes ContentType { get; set; } = ContentTypes.String;

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
