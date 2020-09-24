// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Error codes to communicate when throwing an APIException.
    /// </summary>
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (this class should have been static but we can't change it without breaking binary compat)
    public class ErrorCodes
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        /// <summary>
        /// Other error, not specified.
        /// </summary>
        public const string ServiceError = "ServiceError";

        /// <summary>
        /// Bad argument.
        /// </summary>
        public const string BadArgument = "BadArgument";

        /// <summary>
        /// Error parsing request.
        /// </summary>
        public const string BadSyntax = "BadSyntax";

        /// <summary>
        /// Mandatory property was not specified.
        /// </summary>
        public const string MissingProperty = "MissingProperty";

        /// <summary>
        /// Message exceeded size limits.
        /// </summary>
        public const string MessageSizeTooBig = "MessageSizeTooBig";
    }
}
