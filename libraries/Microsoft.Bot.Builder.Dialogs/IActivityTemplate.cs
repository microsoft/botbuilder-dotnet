using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public interface IActivityTemplate
    {
        Task<Activity> BindToActivity(ITurnContext context, object data);
    }
}
