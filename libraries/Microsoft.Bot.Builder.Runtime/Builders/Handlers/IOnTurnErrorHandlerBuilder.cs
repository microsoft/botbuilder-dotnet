// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Runtime.Builders.Handlers
{
    public interface IOnTurnErrorHandlerBuilder : IBuilder<Func<ITurnContext, Exception, Task>>
    {
    }
}
