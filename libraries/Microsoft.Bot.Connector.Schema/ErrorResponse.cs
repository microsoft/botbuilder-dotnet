// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// An HTTP API response.
    /// </summary>
    public partial class ErrorResponse
    {
        /// <summary>Initializes a new instance of the <see cref="ErrorResponse"/> class.</summary>
        public ErrorResponse()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="ErrorResponse"/> class.</summary>
        /// <param name="error">Error message.</param>
        public ErrorResponse(Error error = default)
        {
            Error = error;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets error message.
        /// </summary>
        /// <value>The error.</value>
        [JsonPropertyName("error")]
        public Error Error { get; set; }

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        partial void CustomInit();
    }
}
