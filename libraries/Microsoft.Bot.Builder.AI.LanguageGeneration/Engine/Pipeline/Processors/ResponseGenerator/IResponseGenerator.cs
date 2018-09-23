using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The blueprint for constructing/generating <see cref="ICompositeResponse"/> objects.
    /// </summary>
    internal interface IResponseGenerator
    {
        /// <summary>
        /// The method responsible for communicating with the Language generation runtime service and construct the <see cref="ICompositeResponse"/> object.
        /// </summary>
        /// <param name="compositeRequest">A <see cref="ICompositeRequest"/> containing the unique template refrences in user request and the slots/entities 
        /// used to resolve (ie : used for substitution inside template resolution values) the referenced templates.</param>
        /// <param name="serviceAgent">The <see cref="IServiceAgent"/> instance uysed to handle the request.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task<ICompositeResponse> GenerateResponseAsync(ICompositeRequest compositeRequest, IServiceAgent serviceAgent);
    }
}
