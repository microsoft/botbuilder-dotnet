// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Runtime.Tests.Components.TestComponents
{
    public class ContosoMiddleware : IMiddleware
    {
        public Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
