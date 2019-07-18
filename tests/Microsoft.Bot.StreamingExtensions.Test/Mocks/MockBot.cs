// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Mocks
{
    public class MockBot : IBot
    {
        public bool ThrowDuringOnTurnAsync { get; set; } = false;

        public List<Activity> Activities { get; set; } = new List<Activity>();

        public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ThrowDuringOnTurnAsync)
            {
                throw new InvalidOperationException();
            }

            Activities.Add(turnContext.Activity);
            return Task.CompletedTask;
        }
    }
}
