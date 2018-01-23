// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Error codes to communicate when throwing an APIException
    /// </summary>
    public class ErrorCodes
    {
        /// <summary>
        /// Other error, not specified
        /// </summary>
        public const string ServiceError = "ServiceError";

        /// <summary>
        /// Bad argument
        /// </summary>
        public const string BadArgument = "BadArgument";

        /// <summary>
        /// Error parsing request
        /// </summary>
        public const string BadSyntax = "BadSyntax";

        /// <summary>
        /// Mandatory property was not specified
        /// </summary>
        public const string MissingProperty = "MissingProperty";

        /// <summary>
        /// Message exceeded size limits
        /// </summary>
        public const string MessageSizeTooBig = "MessageSizeTooBig";
    }
}
