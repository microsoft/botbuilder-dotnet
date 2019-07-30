namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// Hash interface to calculate the attachment hash.
    /// User can inject there only hash method to replace the attachment hash.
    /// Default hash will be MD5.
    /// </summary>
    public interface IAttachmentHash
    {
        /// <summary>
        /// Calculte hash from byte array.
        /// </summary>
        /// <param name="bytes">The byte array need to be hashed.</param>
        /// <returns>The hash string.</returns>
        string Hash(byte[] bytes);

        /// <summary>
        /// Calculte hash from string.
        /// </summary>
        /// <param name="content">The string content need to be hashed.</param>
        /// <returns>The hash string.</returns>
        string Hash(string content);
    }
}
