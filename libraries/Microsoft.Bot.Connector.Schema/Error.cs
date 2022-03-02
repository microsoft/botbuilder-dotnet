// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Object representing error information.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (Cannot change without breaking backwards compatibility.)
    public class Error
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        /// <summary>Initializes a new instance of the <see cref="Error"/> class.</summary>
        public Error()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="Error"/> class.</summary>
        /// <param name="code">Error code.</param>
        /// <param name="message">Error message.</param>
        /// <param name="innerHttpError">Error from inner http call.</param>
        public Error(string code = default, string message = default, InnerHttpError innerHttpError = default)
        {
            Code = code;
            Message = message;
            InnerHttpError = innerHttpError;
            CustomInit();
        }

        /// <summary>Gets or sets error code.</summary>
        /// <value>The error code.</value>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>Gets or sets error message.</summary>
        /// <value>The error message.</value>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>Gets or sets error from inner http call.</summary>
        /// <value>The error from the inner HTTP call.</value>
        [JsonPropertyName("innerHttpError")]
        public InnerHttpError InnerHttpError { get; set; }

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        private void CustomInit()
        {
        }
    }
}
