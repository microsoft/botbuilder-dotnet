using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines Template interface for binding data to T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITemplate<T>
    {
        /// <summary>
        /// Given the turn context bind to the data to create the object of type T
        /// </summary>
        /// <param name="turnContext">TurnContext</param>
        /// <param name="data">data to bind to </param>
        /// <returns>instance of T</returns>
        Task<T> BindToData(ITurnContext turnContext, object data);
    }
}
