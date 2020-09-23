namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines the interface for getting an items identity.
    /// </summary>
    public interface IItemIdentity
    {
        /// <summary>
        /// Gets the identity of the item.
        /// </summary>
        /// <returns>A string representing the identity of the item.</returns>
        string GetIdentity();
    }
}
