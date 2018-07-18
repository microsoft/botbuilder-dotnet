using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// This is metadata about the property including policy info
    /// </summary>
    public interface IPropertyAccessor
    {
        string Name { get; }
    }

    /// <summary>
    /// Interface which defines methods for how you can get data from the context
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPropertyAccessor<T> : IPropertyAccessor
    {
        /// <summary>
        /// Get the property from the context
        /// </summary>
        /// <param name="turnContext"></param>
        /// <returns></returns>
        Task<T> GetAsync(ITurnContext turnContext);

        /// <summary>
        /// Delete the property
        /// </summary>
        /// <param name="turnContext"></param>
        /// <returns></returns>
        Task DeleteAsync(ITurnContext turnContext);

        /// <summary>
        /// Set the property on the context
        /// </summary>
        Task SetAsync(ITurnContext turnContext, T value);
    }
}
