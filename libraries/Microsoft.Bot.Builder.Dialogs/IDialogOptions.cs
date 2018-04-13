
namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Options are arguments to Dialogs and they can be defaulted
    /// </summary>
    public interface IDialogOptions
    {
        object ApplyDefaults(object defaults);
    }
}
