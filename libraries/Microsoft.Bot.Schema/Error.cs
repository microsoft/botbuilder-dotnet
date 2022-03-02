// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;

    /// <summary>
    /// Object representing error information.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (Cannot change without breaking backwards compatibility.)
    public class Error
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        /// <summary>Initializes a new instance of the <see cref="Error"/> class.</summary>
        /// <param name="code">Error code.</param>
        /// <param name="message">Error message.</param>
        /// <param name="innerHttpError">Error from inner http call.</param>
        public Error(string code = default, string message = default, InnerHttpError innerHttpError = default)
        {
            Code = code;
            Message = message;
            InnerHttpError = innerHttpError;
        }

        /// <summary>Gets or sets error code.</summary>
        /// <value>The error code.</value>
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        /// <summary>Gets or sets error message.</summary>
        /// <value>The error message.</value>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>Gets or sets error from inner http call.</summary>
        /// <value>The error from the inner HTTP call.</value>
        [JsonProperty(PropertyName = "innerHttpError")]
        public InnerHttpError InnerHttpError { get; set; }
    }
}
