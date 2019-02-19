using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public interface IActivityGenerator<T>
        where T : IActivity
    {
        Task<T> Generate(string locale, string inlineTemplate, string id, object data, string[] types, string[] tags);
    }

    public interface IMessageGenerator : IActivityGenerator<IMessageActivity>
    {
    }
}
