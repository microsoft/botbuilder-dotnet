namespace Microsoft.Bot.Builder.Dialogs.Composition.Resources
{
    public interface IBotResourceWatcher
    {
        /// <summary>
        /// Fires when the resource source has changed (new resource, changed resource, deleted resource)
        /// </summary>
        event ResourceChangeHandler Changed;
    }
}
