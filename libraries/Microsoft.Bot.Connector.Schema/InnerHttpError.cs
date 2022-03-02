// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Object representing inner http error.
    /// </summary>
    public class InnerHttpError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InnerHttpError"/> class.
        /// </summary>
        public InnerHttpError()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InnerHttpError"/> class.
        /// </summary>
        /// <param name="statusCode">HttpStatusCode from failed request.</param>
        /// <param name="body">Body from failed request.</param>
        public InnerHttpError(int? statusCode = default, object body = default)
        {
            StatusCode = statusCode;
            Body = body;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets httpStatusCode from failed request.
        /// </summary>
        /// <value>The status code of the HTTP request.</value>
        [JsonPropertyName("statusCode")]
        public int? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets body from failed request.
        /// </summary>
        /// <value>The body from failed request.</value>
        [JsonPropertyName("body")]
        public object Body { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
