using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines an IActivityGenerator which returns IMessageActivity.
    /// </summary>
    public interface IMessageActivityGenerator : IActivityGenerator<IMessageActivity>
    {
    }
}
