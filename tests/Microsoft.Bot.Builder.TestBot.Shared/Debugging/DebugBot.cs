// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.TestBot.Shared.Debugging
{
    public class DebugBot : ActivityHandler
    {
        private InspectionMiddleware _inspectionMiddleware;

        public DebugBot(InspectionMiddleware inspectionMiddleware)
        {
            _inspectionMiddleware = inspectionMiddleware;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await _inspectionMiddleware.ProcessCommandAsync(turnContext);
        }
    }
}
