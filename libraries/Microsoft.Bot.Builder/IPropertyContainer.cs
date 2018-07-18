using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IPropertyContainer
    {
        /// <summary>
        /// Get the property from the context.
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="propertyName">name of the property to fetch</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>Task for T</returns>
        Task<T> GetPropertyAsync<T>(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Delete the property.
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="propertyName">name of the property to delete</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeletePropertyAsync(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Set the property on the container.
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="propertyName">name of the property to fetch</param>
        /// <param name="value">value of T to save</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetPropertyAsync(ITurnContext turnContext, string propertyName, object value, CancellationToken cancellationToken = default(CancellationToken));
    }
}
