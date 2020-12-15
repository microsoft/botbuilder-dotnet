// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Runtime.Builders.Handlers
{
    /// <summary>
    /// Defines an interface for an implementation of <see cref="IBuilder{T}"/> that returns an
    /// instance whose type implements <see cref="Func{ITurnContext, Exception, Task}" />.
    /// </summary>
    internal interface IOnTurnErrorHandlerBuilder : IBuilder<Func<ITurnContext, Exception, Task>>
    {
    }
}
