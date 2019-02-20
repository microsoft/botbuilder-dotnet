using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines how to generate an IActivity based on all of the parameters which drive resolution.
    /// </summary>
    /// <typeparam name="T">type of IActivity to return </typeparam>
    public interface IActivityGenerator<T>
        where T : IActivity
    {
        /// <summary>
        /// Generate a IActivity based on paramters
        /// </summary>
        /// <param name="locale">locale</param>
        /// <param name="inlineTemplate">inline expression.</param>
        /// <param name="id">property in template source.</param>
        /// <param name="data">data to bind to.</param>
        /// <param name="types">types to try.</param>
        /// <param name="tags">tags to try.</param>
        /// <returns></returns>
        Task<T> Generate(string locale, string inlineTemplate, string id, object data, string[] types, string[] tags);
    }
}
