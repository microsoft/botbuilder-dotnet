using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IPropertyContainer
    {
        /// <summary>
        /// Get the property from the context
        /// </summary>
        /// <param name="turnContext"></param>
        /// <returns></returns>
        Task<T> GetPropertyAsync<T>(ITurnContext turnContext, string propertyName);

        /// <summary>
        /// Delete the property
        /// </summary>
        /// <param name="turnContext"></param>
        /// <returns></returns>
        Task DeletePropertyAsync(ITurnContext turnContext, string propertyName);

        /// <summary>
        /// Set the property on the container
        /// </summary>
        Task SetPropertyAsync(ITurnContext turnContext, string propertyName, object value);
    }
}
