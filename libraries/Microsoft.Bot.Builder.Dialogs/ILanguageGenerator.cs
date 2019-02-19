using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public interface ILanguageGenerator
    {
        Task<string> Generate(string locale, string inlineTemplate, string id, object data, string[] types, string[] tags);
    }
}
