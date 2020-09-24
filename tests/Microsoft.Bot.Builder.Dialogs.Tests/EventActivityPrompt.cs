// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class EventActivityPrompt : ActivityPrompt
    {
        public EventActivityPrompt(string dialogId, PromptValidator<Activity> validator)
            : base(dialogId, validator)
        {
        }

        public async Task OnPromptNullContext(object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var opt = (PromptOptions)options;

            // should throw ArgumentNullException
            await OnPromptAsync(turnContext: null, state: null, options: opt, isRetry: false);
        }

        public async Task OnPromptNullOptions(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // should throw ArgumentNullException
            await OnPromptAsync(dc.Context, state: null, options: null, isRetry: false);
        }
    }
}
