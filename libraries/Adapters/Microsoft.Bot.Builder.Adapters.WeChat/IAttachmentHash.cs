// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// Hash interface to calculate the attachment hash.
    /// User can inject there only hash method to replace the attachment hash.
    /// </summary>
    public interface IAttachmentHash
    {
        /// <summary>
        /// Calcultes hash from byte array.
        /// </summary>
        /// <param name="bytes">The byte array need to be hashed.</param>
        /// <returns>The hash string.</returns>
        string Hash(byte[] bytes);

        /// <summary>
        /// Calcultes hash from string.
        /// </summary>
        /// <param name="content">The string content need to be hashed.</param>
        /// <returns>The hash string.</returns>
        string Hash(string content);
    }
}
