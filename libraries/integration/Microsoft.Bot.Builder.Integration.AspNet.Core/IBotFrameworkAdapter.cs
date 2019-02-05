// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public interface IBotFrameworkAdapter
    {
        /// <summary>
        /// Interface to express the relationship between an mvc api Controller and a Bot Builder Adapter.
        /// This interface can be used for Dependency Injection.
        /// </summary>
        /// <param name="request">The HTTP request object, typically in a POST handler by a Controller.</param>
        /// <param name="response">The HTTP response object.</param>
        /// <param name="bot">The bot implementation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task ProcessAsync(HttpRequest request, HttpResponse response, IBot bot, CancellationToken cancellationToken = default(CancellationToken));
    }
}
