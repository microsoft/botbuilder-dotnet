// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines Template interface for binding data to T.
    /// </summary>
    /// <typeparam name="T">Type to bind data to.</typeparam>
    public interface ITemplate<T>
    {
        /// <summary>
        /// Given the turn context bind to the data to create the object of type T.
        /// </summary>
        /// <param name="dialogContext">dialogContext.</param>
        /// <param name="data">data to bind to. If Null, then dc.State will be used.</param>
        /// <returns>instance of T.</returns>
        Task<T> BindAsync(DialogContext dialogContext, object data = null);
    }
}
